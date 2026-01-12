namespace UrlShortener.Api.Common.Errors;

public sealed class NotFoundException : ApiException
{
    public NotFoundException(string code, string message) : base(code, message) { }
}