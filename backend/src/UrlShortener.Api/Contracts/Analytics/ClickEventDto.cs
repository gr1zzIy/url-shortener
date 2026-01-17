namespace UrlShortener.Api.Contracts.Analytics;

public sealed record ClickEventDto(
    DateTimeOffset OccurredAt,
    string? IpAddress,
    string? CountryCode,
    string? DeviceType,
    string? Os,
    string? Browser);
