using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Common.Errors;
using UrlShortener.Api.Common.Policies;
using UrlShortener.Api.Contracts.Common;
using UrlShortener.Api.Contracts.Urls;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Persistence;
using Npgsql;

namespace UrlShortener.Api.Services;

public sealed class ShortUrlService
{
    private readonly AppDbContext _db;
    private readonly ShortCodeGenerator _codes;
    
    public ShortUrlService(AppDbContext db, ShortCodeGenerator codes)
    {
        _db = db;
        _codes = codes;
    }

    public async Task<ShortUrlDto> CreateAsync(Guid userId, CreateShortUrlRequest request, CancellationToken ct)
    {
        var custom = request.CustomCode?.Trim();

        // CASE 1: customCode provided -> single insert attempt, 23505 => conflict
        if (!string.IsNullOrWhiteSpace(custom))
        {
            var entity = new ShortUrl
            {
                UserId = userId,
                OriginalUrl = request.OriginalUrl,
                ShortCode = custom,
                ExpiresAt = request.ExpiresAt,
                IsActive = true
            };

            _db.ShortUrls.Add(entity);

            try
            {
                await _db.SaveChangesAsync(ct);
                return ToDto(entity);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new ConflictException(ErrorMessages.ShortCodeAlreadyUsed);
            }
        }

        // CASE 2: generated code -> retry loop, 23505 => retry
        for (int attempt = 0; attempt < ShortCodePolicy.GenerationMaxAttempts; attempt++)
        {
            var code = _codes.Generate();

            var entity = new ShortUrl
            {
                UserId = userId,
                OriginalUrl = request.OriginalUrl,
                ShortCode = code,
                ExpiresAt = request.ExpiresAt,
                IsActive = true
            };

            _db.ShortUrls.Add(entity);

            try
            {
                await _db.SaveChangesAsync(ct);
                return ToDto(entity);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Collision on unique index for ShortCode — retry with a new code.
                _db.ChangeTracker.Clear();
                continue;
            }
        }

        throw new ConflictException(ErrorMessages.FailedToGenerateShortCode);
    }

    public async Task<PagedResult<ShortUrlDto>> ListAsync(Guid userId, int page, int pageSize, CancellationToken ct)
    {
        var (p, ps) = PagingPolicy.Normalize(page, pageSize);

        page = p;
        pageSize = ps;

        var q = _db.ShortUrls
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt);

        var total = await q.LongCountAsync(ct);

        var items = await q.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ShortUrlDto(
                x.Id, x.ShortCode, x.OriginalUrl, x.CreatedAt, x.ExpiresAt, x.IsActive, x.Clicks, x.LastAccessedAt))
            .ToListAsync(ct);

        return new PagedResult<ShortUrlDto>(items, page, pageSize, total);
    }
    
    public async Task<ShortUrlDto> GetAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var entity = await _db.ShortUrls
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId 
                                      && x.Id == id 
                                      && x.DeletedAt == null, ct);

        if (entity is null)
        {
            throw new NotFoundException(ErrorMessages.ShortUrlNotFound);
        }
        
        return new ShortUrlDto(
            entity.Id, entity.ShortCode, entity.OriginalUrl, entity.CreatedAt, entity.ExpiresAt,
            entity.IsActive, entity.Clicks, entity.LastAccessedAt);
    }
    
    public async Task DeactivateAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var entity = await _db.ShortUrls
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && x.DeletedAt == null, ct);

        if (entity is null) throw new NotFoundException(ErrorMessages.ShortUrlNotFound);

        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }
    
    public async Task SoftDeleteAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var entity = await _db.ShortUrls
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && x.DeletedAt == null, ct);

        if (entity is null) throw new NotFoundException(ErrorMessages.ShortUrlNotFound);

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
    
    private static ShortUrlDto ToDto(ShortUrl e) => 
        new (e.Id, e.ShortCode, e.OriginalUrl, e.CreatedAt, e.ExpiresAt, e.IsActive, e.Clicks, e.LastAccessedAt);
    
    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        // Postgres unique violation SQLSTATE is 23505
        return ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}