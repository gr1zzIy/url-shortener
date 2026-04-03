namespace UrlShortener.Api.Contracts.Analytics;

/// <summary>
/// Одна подія переходу за коротким посиланням.
/// </summary>
/// <param name="OccurredAt">Час події.</param>
/// <param name="IpAddress">IP-адреса (якщо зберігається).</param>
/// <param name="CountryCode">Код країни.</param>
/// <param name="DeviceType">Тип пристрою.</param>
/// <param name="Os">Операційна система.</param>
/// <param name="Browser">Браузер.</param>
public sealed record ClickEventDto(
    DateTimeOffset OccurredAt,
    string? IpAddress,
    string? CountryCode,
    string? DeviceType,
    string? Os,
    string? Browser);
