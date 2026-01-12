using UrlShortener.Api.Middleware;

namespace UrlShortener.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApiPipeline(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UrlShortener API v1"));
        }
    
        if (!env.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
    
        app.UseAuthentication();
        app.UseAuthorization();
    
        return app;
    }

}