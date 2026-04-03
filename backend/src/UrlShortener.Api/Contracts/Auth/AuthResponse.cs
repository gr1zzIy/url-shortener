namespace UrlShortener.Api.Contracts.Auth;

/// <summary>
/// Відповідь після успішної автентифікації.
/// </summary>
/// <param name="AccessToken">JWT access token.</param>
/// <param name="TokenType">Тип токена (зазвичай Bearer).</param>
/// <param name="ExpiresIn">Час життя токена в секундах.</param>
public sealed record AuthResponse(string AccessToken, string TokenType, int ExpiresIn);