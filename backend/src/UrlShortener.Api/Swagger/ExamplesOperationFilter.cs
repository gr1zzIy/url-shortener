using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UrlShortener.Api.Contracts.Auth;

public sealed class ExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var route = $"{context.ApiDescription.RelativePath}".ToLowerInvariant();
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();

        // Request examples
        if (method == "POST" && route == "api/auth/register")
        {
            SetRequestExample(operation, new RegisterRequest("user@example.com", "Qwerty123"));
            SetResponseExample(operation, "200", new AuthResponse("eyJhbGciOiJIUzI1NiIsInR5cCI...", "Bearer", 3600));
        }

        if (method == "POST" && route == "api/auth/login")
        {
            SetRequestExample(operation, new LoginRequest("user@example.com", "Qwerty123"));
            SetResponseExample(operation, "200", new AuthResponse("eyJhbGciOiJIUzI1NiIsInR5cCI...", "Bearer", 3600));
        }

        if (method == "GET" && route == "api/auth/me")
        {
            SetResponseExample(operation, "200", new { id = "8f4f3c5a-1f2b-4c4b-9b16-2c8f4a1b2c3d", email = "user@example.com" });
        }
    }

    private static void SetRequestExample(OpenApiOperation operation, object example)
    {
        if (operation.RequestBody?.Content is null) return;

        var json = JsonSerializer.Serialize(example);
        foreach (var contentType in operation.RequestBody.Content.Keys)
        {
            operation.RequestBody.Content[contentType].Example = new OpenApiString(json);
        }
    }

    private static void SetResponseExample(OpenApiOperation operation, string statusCode, object example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out var response)) return;
        if (response.Content is null) return;

        var json = JsonSerializer.Serialize(example);
        foreach (var contentType in response.Content.Keys)
        {
            response.Content[contentType].Example = new OpenApiString(json);
        }
    }
}
