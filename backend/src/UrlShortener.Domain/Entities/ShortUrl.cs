namespace UrlShortener.Domain.Entities;

public sealed class ShortUrl
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }          // FK на ApplicationUser
    public string ShortCode { get; set; } = default!;
    public string OriginalUrl { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    public long Clicks { get; set; }
    public DateTimeOffset? LastAccessedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; } // soft delete
}