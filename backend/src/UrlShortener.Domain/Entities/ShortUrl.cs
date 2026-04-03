namespace UrlShortener.Domain.Entities;

public sealed class ShortUrl
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Власник посилання; усі CRUD-операції фільтруються по цьому полю.
    public Guid UserId { get; set; }

    // Короткий код має бути унікальним серед не видалених посилань.
    public string ShortCode { get; set; } = default!;
    public string OriginalUrl { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    public long Clicks { get; set; }
    public DateTimeOffset? LastAccessedAt { get; set; }

    // Soft delete: запис лишається в БД для історії/аналітики.
    public DateTimeOffset? DeletedAt { get; set; }

    public bool IsDeleted => DeletedAt.HasValue;
}