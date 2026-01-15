namespace UrlShortener.Api.Contracts.Urls;

public sealed record CreateShortUrlRequest(
    string OriginalUrl,
    string? CustomCode,
    DateTimeOffset? ExpiresAt);