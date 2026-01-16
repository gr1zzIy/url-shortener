namespace UrlShortener.Api.Common.Policies;

public static class AuthCookiePolicy
{
    public const string RefreshCookieName = "refresh_token";

    public static CookieOptions BuildRefreshCookieOptions(DateTimeOffset expiresAt, bool isDevelopment)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment, // у проді true обов'язково
            SameSite = SameSiteMode.None, // для різних доменів SPA <-> API
            Expires = expiresAt.UtcDateTime,
            Path = "/api/auth"
        };
    }
}