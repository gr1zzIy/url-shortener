namespace UrlShortener.Api.Contracts.Urls;

/// <summary>
/// Запит на створення короткого посилання.
/// </summary>
/// <param name="OriginalUrl">Оригінальна URL-адреса.</param>
/// <param name="CustomCode">Кастомний короткий код (опційно).</param>
/// <param name="ExpiresAt">Дата й час завершення дії посилання (опційно).</param>
public sealed record CreateShortUrlRequest(
    string OriginalUrl,
    string? CustomCode,
    DateTimeOffset? ExpiresAt);