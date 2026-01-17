namespace UrlShortener.Api.Contracts.Analytics;

public sealed record UrlStatsResponse(
    Guid ShortUrlId,
    string ShortCode,
    DateOnly From,
    DateOnly To,
    long TotalClicks,
    long UniqueVisitors,
    DateTimeOffset? LastAccessedAt,
    IReadOnlyList<UrlStatsPoint> Series);
