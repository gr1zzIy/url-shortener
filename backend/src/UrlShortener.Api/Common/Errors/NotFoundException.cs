namespace UrlShortener.Api.Common.Errors;

public sealed class NotFoundException : ApiException
{
    public NotFoundException(string message) : base(ApiErrorCodes.NotFound, message) { }
}