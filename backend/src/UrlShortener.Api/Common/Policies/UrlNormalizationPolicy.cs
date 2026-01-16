namespace UrlShortener.Api.Common.Policies;

public static class UrlNormalizationPolicy
{
    public static string NormalizeOriginalUrl(string input)
    {
        var s = (input ?? string.Empty).Trim();

        // If user omitted scheme, default to https
        if (!s.Contains("://", StringComparison.Ordinal))
            s = "https://" + s;

        return s;
    }

    public static bool IsValidHttpUrl(string input)
        => Uri.TryCreate(input, UriKind.Absolute, out var u)
           && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
}