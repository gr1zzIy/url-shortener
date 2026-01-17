namespace UrlShortener.Api.Contracts.Analytics;

public sealed record UrlStatsPoint(
    DateOnly Day,
    long Clicks,
    long UniqueClicks);
