namespace UrlShortener.Api.Contracts.Urls;

public sealed record ShortUrlDto(
    Guid Id,
    string ShortCode,
    string OriginalUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    bool IsActive,
    long Clicks,
    DateTimeOffset? LastAccessedAt);