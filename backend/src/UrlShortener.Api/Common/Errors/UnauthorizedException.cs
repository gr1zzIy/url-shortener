namespace UrlShortener.Api.Common.Errors;

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message) : base(ApiErrorCodes.Unauthorized, message) { }
}