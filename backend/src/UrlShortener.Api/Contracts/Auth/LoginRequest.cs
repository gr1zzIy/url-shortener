namespace UrlShortener.Api.Contracts.Auth;

/// <summary>
/// Запит на вхід користувача.
/// </summary>
/// <param name="Email">Email користувача.</param>
/// <param name="Password">Пароль користувача.</param>
public sealed record LoginRequest(string Email, string Password);