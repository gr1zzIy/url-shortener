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
        // На вході завжди нормалізуємо URL, щоб уникати дублів через формат.
        var normalizedUrl = UrlNormalizationPolicy.NormalizeOriginalUrl(request.OriginalUrl);
        
        var custom = request.CustomCode;
        if (!string.IsNullOrWhiteSpace(custom))
            custom = ShortCodePolicy.Normalize(custom);

        // Якщо користувач задав свій код, робимо одну спробу вставки.
        // Конфлікт унікальності тут — очікуваний бізнес-сценарій, а не технічна помилка.
        if (!string.IsNullOrWhiteSpace(custom))
        {
            var entity = new ShortUrl
            {
                UserId = userId,
                OriginalUrl = normalizedUrl,
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

        // Для автогенерації коду допускаємо кілька спроб, бо колізії хоч і рідкі, але можливі.
        for (int attempt = 0; attempt < ShortCodePolicy.GenerationMaxAttempts; attempt++)
        {
            var code = _codes.Generate();

            var entity = new ShortUrl
            {
                UserId = userId,
                OriginalUrl = normalizedUrl,
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
                // Чистимо трекер, інакше невдала сутність залишиться "завислою" між спробами.
                _db.ChangeTracker.Clear();
                continue;
            }
        }

        throw new ConflictException(ErrorMessages.FailedToGenerateShortCode);
    }

    public async Task<PagedResult<ShortUrlDto>> ListAsync(Guid userId, int page, int pageSize, CancellationToken ct)
    {
        // Нормалізація тут захищає і API, і БД від випадкових дуже великих page/pageSize.
        var (pageNorm, pageSizeNorm) = PagingPolicy.Normalize(page, pageSize);
        
        page = pageNorm;
        pageSize = pageSizeNorm;

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
        // Для PostgreSQL код 23505 означає порушення унікального індексу.
        return ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}