using System.Net;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Common.Errors;

namespace UrlShortener.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "API exception: {Code}", ex.Code);
            await WriteProblemDetails(context, MapStatus(ex), ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemDetails(
                context,
                HttpStatusCode.InternalServerError,
                ApiErrorCodes.InternalError,
                "Unexpected error occurred.");
        }
    }

    private static HttpStatusCode MapStatus(ApiException ex) =>
        ex.Code switch
        {
            ApiErrorCodes.NotFound => HttpStatusCode.NotFound,
            ApiErrorCodes.Conflict => HttpStatusCode.Conflict,
            _ => HttpStatusCode.BadRequest
        };

    private static async Task WriteProblemDetails(
        HttpContext context,
        HttpStatusCode status,
        string code,
        string title)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var pd = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Type = $"https://httpstatuses.com/{(int)status}",
            Instance = context.Request.Path
        };

        pd.Extensions["code"] = code;
        pd.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(pd);
    }
}
