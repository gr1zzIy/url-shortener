using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.Api.Common.Errors;

public static class IdentityProblemDetails
{
    public static ValidationProblemDetails ToValidationProblem(
        IdentityResult result,
        HttpContext httpContext,
        string? title = null)
    {
        // Групуємо помилки по Code, щоб фронту було простіше
        var errors = result.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray());

        var pd = new ValidationProblemDetails(errors)
        {
            Type = "https://httpstatuses.com/400",
            Title = title ?? "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Instance = httpContext.Request.Path
        };

        pd.Extensions["code"] = ApiErrorCodes.ValidationFailed;
        pd.Extensions["traceId"] = httpContext.TraceIdentifier;

        return pd;
    }
}