namespace UrlShortener.Api.Contracts.Auth;

public sealed record AuthResponse(string AccessToken, string TokenType, int ExpiresIn);