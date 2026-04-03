using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PingController : ControllerBase
{
    /// <summary>
    /// Перевіряє, що API відповідає та повертає поточний UTC час.
    /// </summary>
    /// <returns>Ознака працездатності API.</returns>
    [HttpGet]
    public IActionResult Get()
        => Ok(new { ok = true, utc = DateTimeOffset.UtcNow });
}
