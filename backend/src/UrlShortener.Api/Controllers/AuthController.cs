using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UrlShortener.Api.Common.Errors;
using UrlShortener.Api.Common.Policies;
using UrlShortener.Api.Contracts.Auth;
using UrlShortener.Api.Services;
using UrlShortener.Infrastructure.Auth;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : BaseApiController
{
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(14);

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwt;

    private readonly AppDbContext _db;
    private readonly RefreshTokenService _refresh;
    private readonly IWebHostEnvironment _env;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwt,
        AppDbContext db,
        RefreshTokenService refresh,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;

        _db = db;
        _refresh = refresh;
        _env = env;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var hasDuplicate = result.Errors.Any(e =>
                e.Code is "DuplicateUserName" or "DuplicateEmail");

            if (hasDuplicate)
            {
                var pd = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/409",
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Instance = HttpContext.Request.Path
                };

                pd.Extensions["code"] = ApiErrorCodes.Conflict;
                pd.Extensions["traceId"] = HttpContext.TraceIdentifier;

                // Щоб фронту було зручно:
                pd.Extensions["errors"] = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray());

                return Conflict(pd);
            }

            return BadRequest(IdentityProblemDetails.ToValidationProblem(result, HttpContext));
        }

        await IssueRefreshTokenAsync(user, ct);

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresMinutes) = _jwt.CreateAccessToken(user, roles);

        return Ok(new AuthResponse(token, "Bearer", expiresMinutes * 60));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return UnauthorizedInvalidCredentials();

        var ok = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!ok.Succeeded)
            return UnauthorizedInvalidCredentials();

        await IssueRefreshTokenAsync(user, ct);

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresMinutes) = _jwt.CreateAccessToken(user, roles);

        return Ok(new AuthResponse(token, "Bearer", expiresMinutes * 60));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken ct)
    {
        var token = Request.Cookies[AuthCookiePolicy.RefreshCookieName];
        if (string.IsNullOrWhiteSpace(token))
            return UnauthorizedRefresh("Missing refresh token.");

        var hash = _refresh.HashToken(token);

        var existing = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (existing is null || !existing.IsActive)
            return UnauthorizedRefresh("Invalid refresh token.");

        // Rotate
        existing.RevokedAt = DateTimeOffset.UtcNow;
        existing.RevokedByIp = GetIp();

        var newToken = _refresh.GenerateToken();
        var newHash = _refresh.HashToken(newToken);
        existing.ReplacedByTokenHash = newHash;

        var newExpiresAt = DateTimeOffset.UtcNow.Add(RefreshLifetime);

        _db.RefreshTokens.Add(new UrlShortener.Domain.Entities.RefreshToken
        {
            UserId = existing.UserId,
            TokenHash = newHash,
            ExpiresAt = newExpiresAt,
            CreatedByIp = GetIp()
        });

        await _db.SaveChangesAsync(ct);

        SetRefreshCookie(newToken, newExpiresAt);

        var user = await _userManager.FindByIdAsync(existing.UserId.ToString());
        if (user is null)
            return UnauthorizedRefresh("Invalid refresh token.");

        var roles = await _userManager.GetRolesAsync(user);
        var (access, expiresMinutes) = _jwt.CreateAccessToken(user, roles);

        return Ok(new AuthResponse(access, "Bearer", expiresMinutes * 60));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = Request.Cookies[AuthCookiePolicy.RefreshCookieName];
        if (!string.IsNullOrWhiteSpace(token))
        {
            var hash = _refresh.HashToken(token);
            var existing = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

            if (existing is not null && existing.IsActive)
            {
                existing.RevokedAt = DateTimeOffset.UtcNow;
                existing.RevokedByIp = GetIp();
                await _db.SaveChangesAsync(ct);
            }
        }

        ClearRefreshCookie();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<object>> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        return Ok(new { user.Id, user.Email });
    }

    // Helpers

    private ActionResult UnauthorizedInvalidCredentials()
    {
        return Unauthorized(new ProblemDetails
        {
            Type = "https://httpstatuses.com/401",
            Title = "Invalid email or password",
            Status = StatusCodes.Status401Unauthorized,
            Instance = HttpContext.Request.Path,
            Extensions =
            {
                ["code"] = ApiErrorCodes.InvalidCredentials,
                ["traceId"] = HttpContext.TraceIdentifier
            }
        });
    }

    private ActionResult UnauthorizedRefresh(string title)
    {
        return Unauthorized(new ProblemDetails
        {
            Type = "https://httpstatuses.com/401",
            Title = title,
            Status = StatusCodes.Status401Unauthorized,
            Instance = HttpContext.Request.Path,
            Extensions =
            {
                ["code"] = ApiErrorCodes.InvalidCredentials,
                ["traceId"] = HttpContext.TraceIdentifier
            }
        });
    }

    private string? GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private void SetRefreshCookie(string refreshToken, DateTimeOffset expiresAt)
    {
        var options = AuthCookiePolicy.BuildRefreshCookieOptions(expiresAt, _env.IsDevelopment());
        Response.Cookies.Append(AuthCookiePolicy.RefreshCookieName, refreshToken, options);
    }

    private void ClearRefreshCookie()
    {
        var options = AuthCookiePolicy.BuildRefreshCookieOptions(DateTimeOffset.UtcNow.AddDays(-1), _env.IsDevelopment());
        Response.Cookies.Delete(AuthCookiePolicy.RefreshCookieName, options);
    }


    private async Task IssueRefreshTokenAsync(ApplicationUser user, CancellationToken ct)
    {
        var token = _refresh.GenerateToken();
        var hash = _refresh.HashToken(token);

        var expiresAt = DateTimeOffset.UtcNow.Add(RefreshLifetime);

        _db.RefreshTokens.Add(new UrlShortener.Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = expiresAt,
            CreatedByIp = GetIp()
        });

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Extremely unlikely, but if hash collides - try again once
            _db.ChangeTracker.Clear();
            token = _refresh.GenerateToken();
            hash = _refresh.HashToken(token);

            _db.RefreshTokens.Add(new UrlShortener.Domain.Entities.RefreshToken
            {
                UserId = user.Id,
                TokenHash = hash,
                ExpiresAt = expiresAt,
                CreatedByIp = GetIp()
            });

            await _db.SaveChangesAsync(ct);
        }

        SetRefreshCookie(token, expiresAt);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
