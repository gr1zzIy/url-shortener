namespace UrlShortener.Api.Common.Errors;

public sealed class ConflictException : ApiException
{
    public ConflictException(string message) : base(ApiErrorCodes.Conflict, message) { }
}