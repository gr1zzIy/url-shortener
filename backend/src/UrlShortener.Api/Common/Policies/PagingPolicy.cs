namespace UrlShortener.Api.Common.Policies;

public static class PagingPolicy
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int page, int pageSize) Normalize(int page, int pageSize)
    {
        var p = page < 1 ? 1 : page;
        var ps = pageSize < 1 ? DefaultPageSize : pageSize;
        ps = ps > MaxPageSize ? MaxPageSize : ps;
        return (p, ps);
    }
}