using System;
using Microsoft.AspNetCore.Http;

namespace UrlShortener.Api.Common.Policies;

public static class AuthCookiePolicy
{
    public const string RefreshCookieName = "refresh_token";

    public static CookieOptions BuildRefreshCookieOptions(DateTimeOffset expiresAt, bool isDevelopment)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.None,
            Expires = expiresAt.UtcDateTime,
            Path = "/api/auth"
        };
    }
}