using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Common.Errors;
using UrlShortener.Api.Common.Policies;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Api.Controllers;

[Route("")]
public sealed class RedirectController : ControllerBase
{
    private readonly AppDbContext _db;

    public RedirectController(AppDbContext db) => _db = db;

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectByCode([FromRoute] string shortCode, CancellationToken ct)
    {
        shortCode = ShortCodePolicy.Normalize(shortCode);
        
        // check if shortcode isn't reserved word
        if (ShortCodePolicy.IsReserved(shortCode))
            throw new NotFoundException(ErrorMessages.ShortUrlNotFound);

        // check if is valid
        if (!ShortCodePolicy.IsValid(shortCode))
        {
            throw new NotFoundException(ErrorMessages.ShortUrlNotFound);
        }

        var now = DateTimeOffset.UtcNow;
        
        // Pull only necessary fields 
        var row = await _db.ShortUrls
            .AsNoTracking()
            .Where(x => x.ShortCode == shortCode
                        && x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.OriginalUrl,
                x.IsActive,
                x.ExpiresAt
            })
            .FirstOrDefaultAsync(ct);

        if (row is null || !row.IsActive)
            throw new NotFoundException(ErrorMessages.ShortUrlNotFound);

        if (row.ExpiresAt is not null && row.ExpiresAt <= now)
            throw new GoneException(ErrorMessages.ShortUrlHasExpired);
        
        // Atomic update
        await _db.ShortUrls
            .Where(x => x.Id == row.Id)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Clicks, x => x.Clicks + 1)
                    .SetProperty(x => x.LastAccessedAt, _ => now),
                ct);

        return Redirect(row.OriginalUrl);
    }
}