using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Common.Auth;
using UrlShortener.Api.Common.Errors;
using System.IdentityModel.Tokens.Jwt;

namespace UrlShortener.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected Guid UserId()
    {
        var raw =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue(AuthClaims.UserId);

        if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var id))
            throw new UnauthorizedException("Unauthorized.");

        return id;
    }

    protected string TraceId => HttpContext.TraceIdentifier;
}