namespace UrlShortener.Api.Contracts.Analytics;

/// <summary>
/// Точка часової серії аналітики.
/// </summary>
/// <param name="Day">День статистики.</param>
/// <param name="Clicks">Кількість усіх кліків за день.</param>
/// <param name="UniqueClicks">Кількість унікальних кліків за день.</param>
public sealed record UrlStatsPoint(
    DateOnly Day,
    long Clicks,
    long UniqueClicks);
