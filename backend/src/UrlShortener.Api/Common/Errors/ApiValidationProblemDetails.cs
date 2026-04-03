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
        // Групую помилки за кодом, щоб відповідь була короткою та передбачуваною.
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

        return ApiProblemDetailsFactory.CreateValidationProblemDetails(httpContext, errors, title);
    }
}