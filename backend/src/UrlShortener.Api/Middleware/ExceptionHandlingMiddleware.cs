using System.Net;
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

            var message = context.RequestServices
                .GetRequiredService<IWebHostEnvironment>()
                .IsDevelopment()
                ? ex.Message
                : ApiResponseConstants.UnexpectedErrorTitle;

            await WriteProblemDetails(
                context,
                HttpStatusCode.InternalServerError,
                ApiErrorCodes.InternalError,
                message);
        }
    }

    private static HttpStatusCode MapStatus(ApiException ex) =>
        ex.Code switch
        {
            ApiErrorCodes.NotFound => HttpStatusCode.NotFound,
            ApiErrorCodes.Conflict => HttpStatusCode.Conflict,
            ApiErrorCodes.Expired => HttpStatusCode.Gone,
            _ => HttpStatusCode.BadRequest
        };

    private static async Task WriteProblemDetails(
        HttpContext context,
        HttpStatusCode status,
        string code,
        string title)
    {
        context.Response.ContentType = ApiResponseConstants.ProblemJsonMediaType;
        context.Response.StatusCode = (int)status;

        var pd = ApiProblemDetailsFactory.CreateProblemDetails(context, (int)status, code, title);

        await context.Response.WriteAsJsonAsync(pd);
    }
}
