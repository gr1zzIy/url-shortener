namespace UrlShortener.Api.Contracts.Analytics;

public sealed record UrlBreakdownResponse(
    Guid ShortUrlId,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<BreakdownItem> Countries,
    IReadOnlyList<BreakdownItem> Devices,
    IReadOnlyList<BreakdownItem> Browsers,
    IReadOnlyList<BreakdownItem> Os);