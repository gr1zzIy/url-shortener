using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UrlShortener.Api.Common.Errors;

public static class ApiProblemDetailsFactory
{
    public static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int statusCode,
        string code,
        string title)
    {
        var pd = new ProblemDetails
        {
            Type = $"{ApiResponseConstants.ProblemTypeBaseUrl}{statusCode}",
            Title = title,
            Status = statusCode,
            Instance = httpContext.Request.Path
        };

        pd.Extensions["code"] = code;
        pd.Extensions["traceId"] = httpContext.TraceIdentifier;
        return pd;
    }

    public static ProblemDetails CreateUnauthorizedProblemDetails(
        HttpContext httpContext,
        string title,
        string code = ApiErrorCodes.InvalidCredentials)
        => CreateProblemDetails(httpContext, StatusCodes.Status401Unauthorized, code, title);

    public static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        IDictionary<string, string[]> errors,
        string? title = null,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        var pd = new ValidationProblemDetails(errors)
        {
            Type = $"{ApiResponseConstants.ProblemTypeBaseUrl}{statusCode}",
            Title = title ?? ApiResponseConstants.ValidationFailedTitle,
            Status = statusCode,
            Instance = httpContext.Request.Path
        };

        pd.Extensions["code"] = ApiErrorCodes.ValidationFailed;
        pd.Extensions["traceId"] = httpContext.TraceIdentifier;
        return pd;
    }

    public static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelState,
        string? title = null,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        var pd = new ValidationProblemDetails(modelState)
        {
            Type = $"{ApiResponseConstants.ProblemTypeBaseUrl}{statusCode}",
            Title = title ?? ApiResponseConstants.ValidationFailedTitle,
            Status = statusCode,
            Instance = httpContext.Request.Path
        };

        pd.Extensions["code"] = ApiErrorCodes.ValidationFailed;
        pd.Extensions["traceId"] = httpContext.TraceIdentifier;
        return pd;
    }
}


