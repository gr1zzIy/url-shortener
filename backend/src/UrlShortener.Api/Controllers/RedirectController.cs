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

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectByCode([FromRoute] string shortCode, CancellationToken ct)
    {
        shortCode = ShortCodePolicy.Normalize(shortCode);

        // Reserved / invalid
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

        var countThisHit = true;

        if (_options.StoreClickEvents)
        {
            var ip = GetClientIp();
            var ua = Request.Headers.UserAgent.ToString();

            var (deviceType, os, browser, isBot) = _enrichment.ParseUserAgent(ua);

            // If bot — do not count/store (optional, but recommended)
            if (isBot)
            {
                countThisHit = false;
            }
            else
            {
                var country = _enrichment.TryGetCountryCode(Request.Headers);
                var visitorHash = _enrichment.ComputeVisitorHash(ip, ua);

                // Unique click window (10 minutes)
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
                        IpAddress = ip, // no masking
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

        // Update counters:
        // - LastAccessedAt always (so you see recent activity)
        // - Clicks only if unique (countThisHit == true)
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
        // Prefer X-Forwarded-For (client, proxy1, proxy2)
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

    // Поки приватність не будемо юзати
    private static string MaskIp(string ip)
    {
        // IPv4: 192.168.1.123 -> 192.168.1.0
        if (ip.Contains('.') && !ip.Contains(':'))
        {
            var parts = ip.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
                return $"{parts[0]}.{parts[1]}.{parts[2]}.0";
        }

        // IPv6: keep first 4 hextets (very rough anonymization)
        if (ip.Contains(':'))
        {
            var parts = ip.Split(':', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(':', parts.Take(Math.Min(parts.Length, 4))) + "::";
        }

        return ip;
    }

    private static string? Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Length <= max ? s : s.Substring(0, max);
    }

    private IActionResult NotFoundResponse(string shortCode, string message)
    {
        if (WantsHtml())
            return Content(HtmlPages.NotFound(shortCode, message), "text/html; charset=utf-8");

        // Keep existing API behavior (ProblemDetails JSON)
        throw new NotFoundException(message);
    }

    private IActionResult GoneResponse(string shortCode, string message, DateTimeOffset? expiresAt)
    {
        if (WantsHtml())
            return Content(HtmlPages.Gone(shortCode, message, expiresAt), "text/html; charset=utf-8");

        throw new GoneException(message);
    }

    private bool WantsHtml()
    {
        // Browser navigation usually sends: text/html in Accept header.
        // API/fetch typically sends application/json.
        var accept = Request.Headers.Accept.ToString();
        return accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static class HtmlPages
    {
        public static string NotFound(string shortCode, string message)
            => Base(
                title: "Link not found",
                headline: "This short link doesn't exist",
                body: $"Code <b>/{Escape(shortCode)}</b> is not available. {Escape(message)}",
                badge: "404"
            );

        public static string Gone(string shortCode, string message, DateTimeOffset? expiresAt)
        {
            var exp = expiresAt.HasValue ? $"<div class=\"muted\">Expired at: {expiresAt:yyyy-MM-dd HH:mm} UTC</div>" : "";
            return Base(
                title: "Link expired",
                headline: "This short link has expired",
                body: $"Code <b>/{Escape(shortCode)}</b> is no longer active. {Escape(message)}{exp}",
                badge: "410"
            );
        }

        private static string Base(string title, string headline, string body, string badge)
        {
            // Small "glass" HTML page. No external assets needed.
            return $@"<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>{Escape(title)}</title>
  <style>
    :root {{
      color-scheme: dark;
      --bg1:#070A12;
      --bg2:#0B1022;
      --card: rgba(255,255,255,0.07);
      --ring: rgba(255,255,255,0.12);
      --text: rgba(255,255,255,0.92);
      --muted: rgba(255,255,255,0.60);
    }}
    body {{
      margin:0;
      min-height:100vh;
      display:grid;
      place-items:center;
      background: radial-gradient(1200px 800px at 20% 10%, rgba(255,255,255,0.10), transparent 55%),
                  radial-gradient(900px 700px at 80% 30%, rgba(255,255,255,0.06), transparent 55%),
                  linear-gradient(180deg, var(--bg1), var(--bg2));
      font-family: ui-sans-serif, system-ui, -apple-system, Segoe UI, Roboto, Helvetica, Arial, ""Apple Color Emoji"",""Segoe UI Emoji"";
      color: var(--text);
    }}
    .card {{
      width:min(720px, calc(100% - 32px));
      border-radius: 24px;
      background: var(--card);
      border: 1px solid var(--ring);
      backdrop-filter: blur(18px);
      box-shadow: 0 30px 90px rgba(0,0,0,0.45);
      padding: 28px;
      position: relative;
      overflow: hidden;
    }}
    .shine {{
      position:absolute; inset:-2px;
      background: radial-gradient(900px 300px at 20% 0%, rgba(255,255,255,0.14), transparent 55%);
      pointer-events:none;
    }}
    .top {{
      display:flex; align-items:center; justify-content:space-between;
      gap: 16px;
    }}
    .badge {{
      font-size: 12px;
      padding: 6px 10px;
      border-radius: 999px;
      border: 1px solid var(--ring);
      color: var(--muted);
      background: rgba(0,0,0,0.22);
    }}
    h1 {{
      margin: 14px 0 0;
      font-size: 26px;
      line-height: 1.2;
    }}
    p {{
      margin: 12px 0 0;
      color: var(--muted);
      line-height: 1.6;
      font-size: 15px;
    }}
    .muted {{
      margin-top: 10px;
      color: var(--muted);
      font-size: 12px;
    }}
    .actions {{
      margin-top: 20px;
      display:flex;
      flex-wrap: wrap;
      gap: 10px;
    }}
    a.btn {{
      display:inline-flex;
      align-items:center;
      justify-content:center;
      padding: 10px 14px;
      border-radius: 16px;
      text-decoration:none;
      border: 1px solid var(--ring);
      background: rgba(255,255,255,0.08);
      color: var(--text);
    }}
    a.btn.primary {{
      background: rgba(255,255,255,0.90);
      color: #0b0f1a;
      border-color: rgba(255,255,255,0.9);
    }}
    a.btn:hover {{
      filter: brightness(1.05);
    }}
  </style>
</head>
<body>
  <div class=""card"">
    <div class=""shine""></div>
    <div class=""top"">
      <div style=""font-weight:600; letter-spacing:-0.2px;"">GlassLink</div>
      <div class=""badge"">{Escape(badge)}</div>
    </div>

    <h1>{headline}</h1>
    <p>{body}</p>

    <div class=""actions"">
      <a class=""btn primary"" href=""/"">Open dashboard</a>
      <a class=""btn"" href=""javascript:history.back()"">Go back</a>
    </div>

    <div class=""muted"">If you believe this is a mistake, contact the owner of this link.</div>
  </div>
</body>
</html>";
        }

        private static string Escape(string s)
            => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
    }
}