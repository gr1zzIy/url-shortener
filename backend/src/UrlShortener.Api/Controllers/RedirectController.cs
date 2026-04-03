using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrlShortener.Api.Common.Errors;
using UrlShortener.Api.Common.Policies;
using UrlShortener.Api.Services;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Api.Controllers;

[Route("")]
public sealed class RedirectController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ClickEnrichmentService _enrichment;
    private readonly AnalyticsOptions _options;

    public RedirectController(AppDbContext db, ClickEnrichmentService enrichment, IOptions<AnalyticsOptions> options)
    {
        _db = db;
        _enrichment = enrichment;
        _options = options.Value;
    }

    /// <summary>
    /// Редіректить за коротким кодом на оригінальну URL-адресу.
    /// </summary>
    /// <param name="shortCode">Короткий код посилання.</param>
    /// <param name="ct">Токен скасування запиту.</param>
    /// <response code="302">Редірект на оригінальну адресу.</response>
    /// <response code="404">Посилання не знайдено або неактивне.</response>
    /// <response code="410">Посилання прострочене.</response>
    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectByCode([FromRoute] string shortCode, CancellationToken ct)
    {
        // Нормалізуємо код один раз на вході, щоб далі працювати з єдиним форматом.
        shortCode = ShortCodePolicy.Normalize(shortCode);

        if (ShortCodePolicy.IsReserved(shortCode) || !ShortCodePolicy.IsValid(shortCode))
            return NotFoundResponse(shortCode, ErrorMessages.ShortUrlNotFound);

        var now = DateTimeOffset.UtcNow;

        var row = await _db.ShortUrls
            .AsNoTracking()
            .Where(x => x.ShortCode == shortCode && x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.OriginalUrl,
                x.IsActive,
                x.ExpiresAt
            })
            .FirstOrDefaultAsync(ct);

        if (row is null || !row.IsActive)
            return NotFoundResponse(shortCode, ErrorMessages.ShortUrlNotFound);

        if (row.ExpiresAt is not null && row.ExpiresAt <= now)
            return GoneResponse(shortCode, ErrorMessages.ShortUrlHasExpired, row.ExpiresAt);

        // countThisHit впливає лише на лічильник кліків, редірект усе одно виконуємо.
        var countThisHit = true;

        if (_options.StoreClickEvents)
        {
            var ip = GetClientIp();
            var ua = Request.Headers.UserAgent.ToString();

            var (deviceType, os, browser, isBot) = _enrichment.ParseUserAgent(ua);

            if (isBot)
            {
                countThisHit = false;
            }
            else
            {
                var country = _enrichment.TryGetCountryCode(Request.Headers);
                var visitorHash = _enrichment.ComputeVisitorHash(ip, ua);

                // В межах короткого вікна не рахуємо повторний клік тим самим visitorHash.
                var cutoff = now.AddMinutes(-10);

                var alreadyCounted = await _db.ClickEvents
                    .AsNoTracking()
                    .AnyAsync(x => x.ShortUrlId == row.Id
                                   && x.VisitorHash == visitorHash
                                   && x.OccurredAt >= cutoff, ct);

                if (alreadyCounted)
                {
                    countThisHit = false;
                }
                else
                {
                    _db.ClickEvents.Add(new ClickEvent
                    {
                        ShortUrlId = row.Id,
                        OccurredAt = now,
                        IpAddress = ip,
                        VisitorHash = visitorHash,
                        UserAgent = Truncate(ua, 512),
                        DeviceType = deviceType,
                        Os = os,
                        Browser = browser,
                        CountryCode = country
                    });

                    await _db.SaveChangesAsync(ct);
                }
            }
        }

        if (countThisHit)
        {
            await _db.ShortUrls
                .Where(x => x.Id == row.Id)
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Clicks, x => x.Clicks + 1)
                        .SetProperty(x => x.LastAccessedAt, _ => now),
                    ct);
        }
        else
        {
            await _db.ShortUrls
                .Where(x => x.Id == row.Id)
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.LastAccessedAt, _ => now),
                    ct);
        }

        return Redirect(row.OriginalUrl);
    }

    private string GetClientIp()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var xff))
        {
            var raw = xff.ToString();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var first = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(first))
                    return first;
            }
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        return string.IsNullOrWhiteSpace(ip) ? "0.0.0.0" : ip;
    }

    private static string? Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Length <= max ? s : s.Substring(0, max);
    }

    private IActionResult NotFoundResponse(string shortCode, string message)
        => RespondWithHtmlOrThrow(RedirectHtmlPages.NotFound(shortCode, message), new NotFoundException(message));

    private IActionResult GoneResponse(string shortCode, string message, DateTimeOffset? expiresAt)
        => RespondWithHtmlOrThrow(RedirectHtmlPages.Gone(shortCode, message, expiresAt), new GoneException(message));

    private IActionResult RespondWithHtmlOrThrow(string html, Exception exception)
    {
        // Для браузера віддаємо HTML-сторінку, для API лишаємо єдиний JSON-формат через middleware.
        if (WantsHtml())
            return Content(html, "text/html; charset=utf-8");

        throw exception;
    }

    private bool WantsHtml()
    {
        var accept = Request.Headers.Accept.ToString();
        return accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }
}