namespace UrlShortener.Domain.Entities;

public sealed class ShortUrl
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ShortCode { get; set; } = default!;
    public string OriginalUrl { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public long Clicks { get; set; }
}
