namespace UrlShortener.Api.Contracts.Urls;

/// <summary>
/// Модель короткого посилання.
/// </summary>
/// <param name="Id">Ідентифікатор посилання.</param>
/// <param name="ShortCode">Короткий код.</param>
/// <param name="OriginalUrl">Оригінальна URL-адреса.</param>
/// <param name="CreatedAt">Дата створення.</param>
/// <param name="ExpiresAt">Дата завершення дії.</param>
/// <param name="IsActive">Ознака активності.</param>
/// <param name="Clicks">Кількість переходів.</param>
/// <param name="LastAccessedAt">Останній час доступу.</param>
public sealed record ShortUrlDto(
    Guid Id,
    string ShortCode,
    string OriginalUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    bool IsActive,
    long Clicks,
    DateTimeOffset? LastAccessedAt);