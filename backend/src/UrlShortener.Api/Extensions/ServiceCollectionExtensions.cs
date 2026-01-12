using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Middleware;

namespace UrlShortener.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // services.AddTransient<ExceptionHandlingMiddleware>();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Type = "https://httpstatuses.com/400",
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.HttpContext.Request.Path
                };

                problemDetails.Extensions["code"] = Common.Errors.ApiErrorCodes.ValidationFailed;
                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                
                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });
        
        return services;
    }
}