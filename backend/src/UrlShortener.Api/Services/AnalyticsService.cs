using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Common.Errors;
using UrlShortener.Api.Contracts.Analytics;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Api.Services;

public sealed class AnalyticsService
{
    private readonly AppDbContext _db;

    public AnalyticsService(AppDbContext db) => _db = db;

    public async Task<UrlStatsResponse> GetStatsAsync(Guid userId, Guid shortUrlId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        if (to < from)
            (from, to) = (to, from);

        // Ensure ownership
        var url = await _db.ShortUrls
            .AsNoTracking()
            .Where(x => x.Id == shortUrlId && x.UserId == userId && x.DeletedAt == null)
            .Select(x => new { x.Id, x.ShortCode, x.Clicks, x.LastAccessedAt, x.CreatedAt })
            .FirstOrDefaultAsync(ct);

        if (url is null)
            throw new NotFoundException(ErrorMessages.ShortUrlNotFound);

        var fromDt = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDt = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var fromDto = new DateTimeOffset(fromDt, TimeSpan.Zero);
        var toDto = new DateTimeOffset(toDt, TimeSpan.Zero);

        var baseQ = _db.ClickEvents
            .AsNoTracking()
            .Where(e => e.ShortUrlId == shortUrlId && e.OccurredAt >= fromDto && e.OccurredAt < toDto);

        // Totals
        var totalClicks = await baseQ.LongCountAsync(ct);
        var uniqueVisitors = await baseQ
            .Select(x => x.VisitorHash)
            .Distinct()
            .LongCountAsync(ct);

        // Daily series
        var dailyRaw = await baseQ
            .GroupBy(x => new
            {
                Y = x.OccurredAt.UtcDateTime.Year,
                M = x.OccurredAt.UtcDateTime.Month,
                D = x.OccurredAt.UtcDateTime.Day
            })
            .Select(g => new
            {
                g.Key.Y,
                g.Key.M,
                g.Key.D,
                Clicks = g.LongCount(),
                Unique = g.Select(x => x.VisitorHash).Distinct().LongCount()
            })
            .ToListAsync(ct);

        // map to DateOnly
        var map = dailyRaw.ToDictionary(
            x => new DateOnly(x.Y, x.M, x.D),
            x => x
        );

        var points = new List<UrlStatsPoint>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (map.TryGetValue(d, out var v))
                points.Add(new UrlStatsPoint(d, v.Clicks, v.Unique));
            else
                points.Add(new UrlStatsPoint(d, 0, 0));
        }

        return new UrlStatsResponse(
            shortUrlId,
            url.ShortCode,
            from,
            to,
            totalClicks,
            uniqueVisitors,
            url.LastAccessedAt,
            points);
    }

    public async Task<UrlBreakdownResponse> GetBreakdownAsync(
    Guid userId,
    Guid shortUrlId,
    DateOnly from,
    DateOnly to,
    CancellationToken ct)
{
    if (to < from)
        (from, to) = (to, from);

    var owns = await _db.ShortUrls
        .AsNoTracking()
        .AnyAsync(x => x.Id == shortUrlId && x.UserId == userId && x.DeletedAt == null, ct);

    if (!owns)
        throw new NotFoundException(ErrorMessages.ShortUrlNotFound);

    var fromDt = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    var toDt = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    // DateTimeOffset bounds to match OccurredAt type (DateTimeOffset)
    var fromDto = new DateTimeOffset(fromDt, TimeSpan.Zero);
    var toDto = new DateTimeOffset(toDt, TimeSpan.Zero);

    var q = _db.ClickEvents
        .AsNoTracking()
        .Where(e => e.ShortUrlId == shortUrlId && e.OccurredAt >= fromDto && e.OccurredAt < toDto);

    // Countries
    var countriesRaw = await q
        .GroupBy(x => x.CountryCode ?? "??")
        .Select(g => new { Key = g.Key, Count = g.LongCount() })
        .OrderByDescending(x => x.Count)
        .Take(20)
        .ToListAsync(ct);

    var countries = countriesRaw
        .Select(x => new BreakdownItem(x.Key, x.Count))
        .ToList();

    // Devices
    var devicesRaw = await q
        .GroupBy(x => x.DeviceType ?? "unknown")
        .Select(g => new { Key = g.Key, Count = g.LongCount() })
        .OrderByDescending(x => x.Count)
        .Take(20)
        .ToListAsync(ct);

    var devices = devicesRaw
        .Select(x => new BreakdownItem(x.Key, x.Count))
        .ToList();

    // Browsers
    var browsersRaw = await q
        .GroupBy(x => x.Browser ?? "unknown")
        .Select(g => new { Key = g.Key, Count = g.LongCount() })
        .OrderByDescending(x => x.Count)
        .Take(20)
        .ToListAsync(ct);

    var browsers = browsersRaw
        .Select(x => new BreakdownItem(x.Key, x.Count))
        .ToList();

    // OS
    var osRaw = await q
        .GroupBy(x => x.Os ?? "unknown")
        .Select(g => new { Key = g.Key, Count = g.LongCount() })
        .OrderByDescending(x => x.Count)
        .Take(20)
        .ToListAsync(ct);

    var os = osRaw
        .Select(x => new BreakdownItem(x.Key, x.Count))
        .ToList();

    return new UrlBreakdownResponse(shortUrlId, from, to, countries, devices, browsers, os);
}



    public async Task<IReadOnlyList<ClickEventDto>> GetRecentClicksAsync(Guid userId, Guid shortUrlId, int take, CancellationToken ct)
    {
        take = Math.Clamp(take, 1, 200);

        var owns = await _db.ShortUrls
            .AsNoTracking()
            .AnyAsync(x => x.Id == shortUrlId && x.UserId == userId && x.DeletedAt == null, ct);

        if (!owns)
            throw new NotFoundException(ErrorMessages.ShortUrlNotFound);

        return await _db.ClickEvents
            .AsNoTracking()
            .Where(x => x.ShortUrlId == shortUrlId)
            .OrderByDescending(x => x.OccurredAt)
            .Take(take)
            .Select(x => new ClickEventDto(
                x.OccurredAt,
                x.IpAddress,
                x.CountryCode,
                x.DeviceType,
                x.Os,
                x.Browser))
            .ToListAsync(ct);
    }
}
