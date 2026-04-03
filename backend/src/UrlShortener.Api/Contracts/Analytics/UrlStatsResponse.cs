namespace UrlShortener.Api.Contracts.Analytics;

/// <summary>
/// Зведена статистика за коротким посиланням за вибраний період.
/// </summary>
/// <param name="ShortUrlId">Ідентифікатор короткого посилання.</param>
/// <param name="ShortCode">Короткий код посилання.</param>
/// <param name="From">Початок періоду.</param>
/// <param name="To">Кінець періоду.</param>
/// <param name="TotalClicks">Загальна кількість кліків.</param>
/// <param name="UniqueVisitors">Кількість унікальних відвідувачів.</param>
/// <param name="LastAccessedAt">Останній час переходу.</param>
/// <param name="Series">Часова серія статистики.</param>
public sealed record UrlStatsResponse(
    Guid ShortUrlId,
    string ShortCode,
    DateOnly From,
    DateOnly To,
    long TotalClicks,
    long UniqueVisitors,
    DateTimeOffset? LastAccessedAt,
    IReadOnlyList<UrlStatsPoint> Series);
