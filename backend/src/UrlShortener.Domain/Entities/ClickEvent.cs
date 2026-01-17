namespace UrlShortener.Domain.Entities;

public sealed class ClickEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShortUrlId { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    // Can be stored as-is or masked depending on configuration.
    public string? IpAddress { get; set; }

    // SHA-256 hex of (ip + userAgent) to support unique visitor counting without relying on raw IP.
    public string VisitorHash { get; set; } = default!;

    public string? UserAgent { get; set; }

    public string? DeviceType { get; set; } // desktop / mobile / tablet / bot / unknown
    public string? Os { get; set; }
    public string? Browser { get; set; }

    public string? CountryCode { get; set; } // ISO-3166-1 alpha-2 if available
}
