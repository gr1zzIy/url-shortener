namespace UrlShortener.Api.Contracts.Analytics;

/// <summary>
/// Відповідь із деталізацією аналітики за категоріями.
/// </summary>
/// <param name="ShortUrlId">Ідентифікатор короткого посилання.</param>
/// <param name="From">Початок періоду.</param>
/// <param name="To">Кінець періоду.</param>
/// <param name="Countries">Розподіл за країнами.</param>
/// <param name="Devices">Розподіл за типами пристроїв.</param>
/// <param name="Browsers">Розподіл за браузерами.</param>
/// <param name="Os">Розподіл за операційними системами.</param>
public sealed record UrlBreakdownResponse(
    Guid ShortUrlId,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<BreakdownItem> Countries,
    IReadOnlyList<BreakdownItem> Devices,
    IReadOnlyList<BreakdownItem> Browsers,
    IReadOnlyList<BreakdownItem> Os);