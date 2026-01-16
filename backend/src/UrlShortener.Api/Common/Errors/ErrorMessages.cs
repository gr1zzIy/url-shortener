namespace UrlShortener.Api.Common.Errors;

public static class ErrorMessages
{
    public const string ShortUrlNotFound = "Short URL not found.";
    public const string ShortCodeAlreadyUsed = "Short code is already in use.";
    public const string FailedToGenerateShortCode = "Failed to generate unique short code.";
    public const string ShortUrlHasExpired = "Short URL has expired.";
}