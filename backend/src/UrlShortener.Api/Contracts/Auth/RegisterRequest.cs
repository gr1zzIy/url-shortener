namespace UrlShortener.Api.Contracts.Auth;

/// <summary>
/// Запит на реєстрацію нового користувача.
/// </summary>
/// <param name="Email">Email користувача.</param>
/// <param name="Password">Пароль користувача.</param>
public sealed record RegisterRequest(string Email, string Password);