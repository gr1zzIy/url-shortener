namespace UrlShortener.Api.Common.Policies;

public static class UrlPolicy
{
    public const int MaxOriginalUrlLength = 2048;

    public static bool IsHttpOrHttps(Uri u) =>
        u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps;
}