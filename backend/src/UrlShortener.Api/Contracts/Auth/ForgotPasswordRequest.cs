namespace UrlShortener.Api.Contracts.Auth;

/// <summary>
/// Запит на ініціацію скидання пароля.
/// </summary>
/// <param name="Email">Email користувача.</param>
public sealed record ForgotPasswordRequest(string Email);
