using System.Collections.Generic;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
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
        // Group errors by code to keep API responses compact and predictable.
        var errors = new Dictionary<string, string[]>();
        var groupedErrors = new Dictionary<string, List<string>>();

        foreach (var error in result.Errors)
        {
            if (!groupedErrors.TryGetValue(error.Code, out var messages))
            {
                messages = new List<string>();
                groupedErrors[error.Code] = messages;
            }

            messages.Add(error.Description);
        }

        foreach (var pair in groupedErrors)
        {
            errors[pair.Key] = pair.Value.ToArray();
        }

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