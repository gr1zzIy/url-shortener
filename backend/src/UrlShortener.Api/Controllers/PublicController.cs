using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Common.Errors;
using UrlShortener.Api.Common.Policies;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/public")]
public sealed class PublicController : ControllerBase
{
    private readonly AppDbContext _db;
    public PublicController(AppDbContext db) => _db = db;

    [HttpGet("resolve/{shortCode}")]
    public async Task<IActionResult> Resolve(string shortCode, CancellationToken ct)
    {
        shortCode = ShortCodePolicy.Normalize(shortCode);

        if (!ShortCodePolicy.IsValid(shortCode) || ShortCodePolicy.IsReserved(shortCode))
            throw new NotFoundException(ErrorMessages.ShortUrlNotFound);

        var now = DateTimeOffset.UtcNow;

        var row = await _db.ShortUrls
            .AsNoTracking()
            .Where(x => x.ShortCode == shortCode && x.DeletedAt == null)
            .Select(x => new
            {
                x.OriginalUrl,
                x.IsActive,
                x.ExpiresAt
            })
            .FirstOrDefaultAsync(ct);

        if (row is null || !row.IsActive)
            throw new NotFoundException(ErrorMessages.ShortUrlNotFound);

        if (row.ExpiresAt is not null && row.ExpiresAt <= now)
            throw new GoneException(ErrorMessages.ShortUrlHasExpired);

        return Ok(new
        {
            shortCode,
            row.OriginalUrl,
            row.ExpiresAt
        });
    }
}