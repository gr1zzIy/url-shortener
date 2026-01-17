using System.Security.Cryptography;
using System.Text;

namespace UrlShortener.Api.Services;

public sealed class ClickEnrichmentService
{
    public string ComputeVisitorHash(string ip, string? userAgent)
    {
        var raw = $"{ip}|{userAgent ?? string.Empty}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public (string DeviceType, string? Os, string? Browser, bool IsBot) ParseUserAgent(string? ua)
    {
        if (string.IsNullOrWhiteSpace(ua))
            return ("unknown", null, null, false);

        var s = ua.ToLowerInvariant();

        var isBot = s.Contains("bot") || s.Contains("spider") || s.Contains("crawl") || s.Contains("slurp");
        if (isBot)
            return ("bot", null, null, true);

        string? os = null;
        if (s.Contains("windows")) os = "Windows";
        else if (s.Contains("mac os") || s.Contains("macintosh")) os = "macOS";
        else if (s.Contains("android")) os = "Android";
        else if (s.Contains("iphone") || s.Contains("ipad") || s.Contains("ios")) os = "iOS";
        else if (s.Contains("linux")) os = "Linux";

        string? browser = null;
        if (s.Contains("edg/")) browser = "Edge";
        else if (s.Contains("chrome/") && !s.Contains("chromium")) browser = "Chrome";
        else if (s.Contains("safari/") && !s.Contains("chrome/")) browser = "Safari";
        else if (s.Contains("firefox/")) browser = "Firefox";
        else if (s.Contains("opr/") || s.Contains("opera")) browser = "Opera";

        var deviceType = "desktop";
        if (s.Contains("ipad") || s.Contains("tablet")) deviceType = "tablet";
        else if (s.Contains("mobile") || s.Contains("iphone") || s.Contains("android")) deviceType = "mobile";

        return (deviceType, os, browser, false);
    }

    public string? TryGetCountryCode(IHeaderDictionary headers)
    {
        // Cloudflare: CF-IPCountry
        if (headers.TryGetValue("CF-IPCountry", out var cf) && !string.IsNullOrWhiteSpace(cf))
            return NormalizeCountry(cf.ToString());

        // Some proxies: X-Country-Code
        if (headers.TryGetValue("X-Country-Code", out var xcc) && !string.IsNullOrWhiteSpace(xcc))
            return NormalizeCountry(xcc.ToString());

        // Render/other: sometimes provides X-Geo-Country
        if (headers.TryGetValue("X-Geo-Country", out var xgc) && !string.IsNullOrWhiteSpace(xgc))
            return NormalizeCountry(xgc.ToString());

        return null;
    }

    private static string? NormalizeCountry(string raw)
    {
        var cc = raw.Trim().ToUpperInvariant();
        if (cc.Length != 2) return null;
        if (cc == "XX" || cc == "ZZ") return null;
        return cc;
    }
}
