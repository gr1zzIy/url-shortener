namespace UrlShortener.Api.Contracts.Analytics;

/// <summary>
/// Елемент агрегації для зрізів аналітики.
/// </summary>
/// <param name="Key">Назва категорії.</param>
/// <param name="Count">Кількість подій у категорії.</param>
public sealed record BreakdownItem(string Key, long Count);
