namespace UrlShortener.Api.Common.Errors;

public class ApiErrorCodes
{
    public const string ValidationFailed = "validation_failed";
    public const string NotFound = "not_found";
    public const string Conflict = "conflict";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string InternalError = "internal_error";
    public const string InvalidCredentials = "invalid_credentials";
    public const string Expired = "expired";
}