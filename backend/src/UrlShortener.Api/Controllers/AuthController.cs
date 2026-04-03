using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwt,
        AppDbContext db,
        RefreshTokenService refresh,
        IWebHostEnvironment env,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;

        _db = db;
        _refresh = refresh;
        _env = env;
        _config = config;
    }

    /// <summary>
    /// Реєструє нового користувача та повертає access token.
    /// </summary>
    /// <param name="request">Дані для реєстрації користувача.</param>
    /// <param name="ct">Токен скасування запиту.</param>
    /// <response code="200">Користувача успішно створено.</response>
    /// <response code="400">Помилка валідації даних.</response>
    /// <response code="409">Користувач з таким email вже існує.</response>
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
            var (status, code, title, errors) = IdentityErrorMapper.ToProblem(result);
            var pd = ApiProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, errors, title, status);
            pd.Extensions["code"] = code;

            return StatusCode(status, pd);
        }

        await IssueRefreshTokenAsync(user, ct);

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresMinutes) = _jwt.CreateAccessToken(user, roles);

        return Ok(new AuthResponse(token, "Bearer", expiresMinutes * 60));
    }

    /// <summary>
    /// Виконує вхід користувача за email і паролем.
    /// </summary>
    /// <param name="request">Облікові дані користувача.</param>
    /// <param name="ct">Токен скасування запиту.</param>
    /// <response code="200">Успішна автентифікація.</response>
    /// <response code="401">Неправильний email або пароль.</response>
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

    /// <summary>
    /// Ініціює скидання пароля.
    /// </summary>
    /// <param name="request">Email користувача для скидання пароля.</param>
    /// <param name="ct">Токен скасування запиту.</param>
    /// <response code="200">Запит прийнято; у development може повертатись reset URL.</response>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<object>> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Ok(new { message = "If the email exists, a reset link will be sent." });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var clientBaseUrl = _config["Client:BaseUrl"] ?? "http://localhost:5000";
        var url = BuildResetUrl(clientBaseUrl, request.Email, token);

        if (_env.EnvironmentName == Environments.Development)
            return Ok(new { message = "Reset link generated.", resetUrl = url });

        // У продакшені тут відправимо листа.
        return Ok(new { message = "If the email exists, a reset link will be sent." });
    }

    /// <summary>
    /// Завершує скидання пароля за токеном.
    /// </summary>
    /// <param name="request">Email, токен та новий пароль.</param>
    /// <param name="ct">Токен скасування запиту.</param>
    /// <response code="200">Пароль успішно змінено.</response>
    /// <response code="400">Токен або новий пароль невалідні.</response>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Ok(new { message = "Password reset completed." });

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var (status, code, title, errors) = IdentityErrorMapper.ToProblem(result);
            var pd = ApiProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, errors, title, status);
            pd.Extensions["code"] = code;

            return StatusCode(status, pd);
        }

        // Обнуляємо активні refresh-токени, щоб після reset не лишався старий доступ.
        await _db.RefreshTokens
            .Where(x => x.UserId == user.Id && x.RevokedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.RevokedAt, _ => DateTimeOffset.UtcNow)
                    .SetProperty(x => x.RevokedByIp, _ => GetIp()),
                ct);

        ClearRefreshCookie();
        return Ok(new { message = "Password reset completed." });
    }

    /// <summary>
    /// Оновлює access token за активним refresh token у cookie.
    /// </summary>
    /// <param name="ct">Токен скасування запиту.</param>
    /// <response code="200">Новий access token успішно видано.</response>
    /// <response code="401">Refresh token відсутній або невалідний.</response>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken ct)
    {
        // Refresh token живе тільки в cookie — це зменшує ризик витоку в JS-клієнті.
        var token = Request.Cookies[AuthCookiePolicy.RefreshCookieName];
        if (string.IsNullOrWhiteSpace(token))
            return UnauthorizedRefresh("Missing refresh token.");

        var hash = _refresh.HashToken(token);

        var existing = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (existing is null || !existing.IsActive)
            return UnauthorizedRefresh("Invalid refresh token.");

        // Ротація: старий токен відкликаємо, новий видаємо в межах того ж запиту.
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

    /// <summary>
    /// Виконує вихід користувача та відкликає refresh token.
    /// </summary>
    /// <param name="ct">Токен скасування запиту.</param>
    /// <response code="204">Вихід успішно виконано.</response>
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

    /// <summary>
    /// Повертає дані поточного авторизованого користувача.
    /// </summary>
    /// <response code="200">Інформацію про користувача повернуто.</response>
    /// <response code="401">Користувач не авторизований.</response>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<object>> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        return Ok(new { user.Id, user.Email });
    }

    // Допоміжні методи

    private ActionResult UnauthorizedInvalidCredentials()
    {
        return Unauthorized(ApiProblemDetailsFactory.CreateUnauthorizedProblemDetails(
            HttpContext,
            ApiResponseConstants.InvalidCredentialsTitle));
    }

    private ActionResult UnauthorizedRefresh(string title)
    {
        return Unauthorized(ApiProblemDetailsFactory.CreateUnauthorizedProblemDetails(HttpContext, title));
    }

    private string? GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private void SetRefreshCookie(string refreshToken, DateTimeOffset expiresAt)
    {
        // В одному місці формуємо cookie-політику, щоб не роз'їжджалась між endpoint-ами.
        var options = AuthCookiePolicy.BuildRefreshCookieOptions(expiresAt, _env.EnvironmentName == Environments.Development);
        Response.Cookies.Append(AuthCookiePolicy.RefreshCookieName, refreshToken, options);
    }

    private void ClearRefreshCookie()
    {
        var options = AuthCookiePolicy.BuildRefreshCookieOptions(DateTimeOffset.UtcNow.AddDays(-1), _env.EnvironmentName == Environments.Development);
        Response.Cookies.Delete(AuthCookiePolicy.RefreshCookieName, options);
    }


    private async Task IssueRefreshTokenAsync(ApplicationUser user, CancellationToken ct)
    {
        // Первинну видачу токена ізолюємо в окремий метод, бо його викликаємо і при register, і при login.
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

    private static string BuildResetUrl(string baseUrl, string email, string token)
    {
        // Токен може містити символи +/=, тому його треба кодувати.
        var client = baseUrl.TrimEnd('/');
        var e = Uri.EscapeDataString(email);
        var t = Uri.EscapeDataString(token);
        return $"{client}/reset-password?email={e}&token={t}";
    }
}
