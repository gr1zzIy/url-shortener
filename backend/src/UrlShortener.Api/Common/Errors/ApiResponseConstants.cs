namespace UrlShortener.Api.Common.Errors;

public static class ApiResponseConstants
{
    public const string ProblemJsonMediaType = "application/problem+json";
    public const string ProblemTypeBaseUrl = "https://httpstatuses.com/";
    public const string ValidationFailedTitle = "Validation failed";
    public const string ConflictTitle = "Conflict";
    public const string InvalidCredentialsTitle = "Invalid email or password";
    public const string UnexpectedErrorTitle = "Unexpected error occurred.";
}

