namespace UrlShortener.Api.Common.Errors;

public sealed class ConflictException : ApiException
{
    public ConflictException(string code, string message) : base(code, message) { }
}