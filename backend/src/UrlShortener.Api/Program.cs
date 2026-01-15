using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using UrlShortener.Api.Extensions;
using UrlShortener.Api.Services;
using UrlShortener.Infrastructure.Auth;
using UrlShortener.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

#region Для контейнера Docker

// Конфігурація для контейнера
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    builder.Configuration.AddJsonFile("appsettings.Container.json", optional: true);
}

// Налаштування DataProtection для контейнера
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Configuration["DataProtection:KeyRingPath"] ?? "/app/keys"));

#endregion

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Controllers + Swagger (Bearer)
builder.Services.AddControllers();
builder.Services.AddSwaggerWithAuth();

// Database
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Identity
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
var secret = jwt.GetValue<string>("Secret") ?? throw new InvalidOperationException("Jwt:Secret is missing");
var issuer = jwt.GetValue<string>("Issuer") ?? throw new InvalidOperationException("Jwt:Issuer is missing");
var audience = jwt.GetValue<string>("Audience") ?? throw new InvalidOperationException("Jwt:Audience is missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// API services: FluentValidation + ProblemDetails for model binding
builder.Services.AddApiServices();

// Token service
builder.Services.AddSingleton<JwtTokenService>();

// Health
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("db");

// ShortCode generator
builder.Services.AddSingleton(new ShortCodeGenerator(length: 8));

builder.Services.AddScoped<ShortUrlService>();
builder.Services.AddSingleton(new ShortCodeGenerator(8));

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseApiPipeline(app.Environment);

app.MapHealthChecks("/health");
app.MapControllers();

#region Для контейнера Docker

// Автоматичні міграції ТІЛЬКИ в контейнері

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true") 
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Applying database migrations in container...");

    // Чекаємо на БД
    var maxRetries = 10;
    Exception? last = null;

    for (var i = 0; i < maxRetries; i++)
    {
        try
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully!");
            last = null;
            break;
        }
        catch (Npgsql.NpgsqlException ex) when (i < maxRetries - 1) 
        {
            last = ex;
            logger.LogWarning("Database not ready yet ({Attempt}/{Max}): {Message}", i + 1, maxRetries, ex.Message);
            await Task.Delay(2000); 
        }
            
    }
    
    if (last != null) 
        throw new InvalidOperationException("Database migrations failed after retries.", last);
}


#endregion

app.Run();
