namespace UrlShortener.Api.Contracts.Auth;

/// <summary>
/// Запит на завершення скидання пароля.
/// </summary>
/// <param name="Email">Email користувача.</param>
/// <param name="Token">Токен скидання пароля.</param>
/// <param name="NewPassword">Новий пароль.</param>
public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword);
