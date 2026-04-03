namespace UrlShortener.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    // Зберігаємо лише хеш токена, щоб сирий токен не лежав у БД.
    public string TokenHash { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
    // Ланцюжок ротації: який токен прийшов на заміну цьому.
    public string? ReplacedByTokenHash { get; set; }

    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }

    // Прапорці нижче — концентрована логіка стану токена для сервісного шару.
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsRevoked && !IsExpired;
}