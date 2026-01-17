namespace UrlShortener.Api.Services;

public sealed class AnalyticsOptions
{
    public bool StoreFullIp { get; set; } = false;

    // If true, the redirect endpoint stores every click event.
    // If false, the system only increments aggregate counters on ShortUrl.
    public bool StoreClickEvents { get; set; } = true;
}
