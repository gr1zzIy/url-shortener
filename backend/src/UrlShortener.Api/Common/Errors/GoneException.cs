namespace UrlShortener.Api.Common.Errors;

public sealed class GoneException : ApiException
{
    public GoneException(string message) : base(ApiErrorCodes.Expired, message) { }
}