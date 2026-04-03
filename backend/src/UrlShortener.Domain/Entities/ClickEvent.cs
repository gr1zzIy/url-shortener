namespace UrlShortener.Domain.Entities;

public sealed class ClickEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ShortUrlId { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    // IP може зберігатися як є або в анонімізованому вигляді — залежить від політики збирання.
    public string? IpAddress { get; set; }

    // SHA-256(ip + userAgent): це основа для підрахунку унікальних кліків без прив'язки до сирого IP.
    public string VisitorHash { get; set; } = default!;

    public string? UserAgent { get; set; }

    // desktop / mobile / tablet / bot / unknown
    public string? DeviceType { get; set; }
    public string? Os { get; set; }
    public string? Browser { get; set; }

    // ISO-3166-1 alpha-2, якщо вдалося визначити країну.
    public string? CountryCode { get; set; }
}
