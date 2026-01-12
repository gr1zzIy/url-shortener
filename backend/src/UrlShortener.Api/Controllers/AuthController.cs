using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Common.Errors;
using UrlShortener.Api.Services;
using UrlShortener.Infrastructure.Auth;
using UrlShortener.Api.Contracts.Auth;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwt;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwt)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
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
            var (status, code, errors) = IdentityErrorMapper.ToProblem(result);

            if (status == StatusCodes.Status400BadRequest)
            {
                var vpd = new ValidationProblemDetails(errors)
                {
                    Type = "https://httpstatuses.com/400",
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                };

                vpd.Extensions["code"] = code;
                vpd.Extensions["traceId"] = HttpContext.TraceIdentifier;

                return BadRequest(vpd);
            }

            // 409 conflict
            var pd = new ProblemDetails
            {
                Type = "https://httpstatuses.com/409",
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            };

            pd.Extensions["code"] = code;
            pd.Extensions["traceId"] = HttpContext.TraceIdentifier;
            pd.Extensions["errors"] = errors;

            return Conflict(pd);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresMinutes) = _jwt.CreateAccessToken(user, roles);

        return Ok(new AuthResponse(token, "Bearer", expiresMinutes * 60));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
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

        var ok = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!ok.Succeeded)
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

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresMinutes) = _jwt.CreateAccessToken(user, roles);

        return Ok(new AuthResponse(token, "Bearer", expiresMinutes * 60));
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<object>> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        return Ok(new { user.Id, user.Email });
    }

}
