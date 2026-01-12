using UrlShortener.Api.Middleware;

namespace UrlShortener.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApiPipeline(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UrlShortener API v1"));
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        
        app.UseAuthorization();
        
        return app;
    }
}