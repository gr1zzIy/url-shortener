using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Common.Auth;
using UrlShortener.Api.Contracts.Common;
using UrlShortener.Api.Contracts.Urls;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/urls")]
public sealed class UrlsController : BaseApiController
{
    private readonly ShortUrlService _service;

    public UrlsController(ShortUrlService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<ShortUrlDto>> Create(CreateShortUrlRequest request, CancellationToken ct)
    {
        var userId = UserId();
        var created = await _service.CreateAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ShortUrlDto>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userId = UserId();
        var result = await _service.ListAsync(userId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShortUrlDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = UserId();
        return Ok(await _service.GetAsync(userId, id, ct));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var userId = UserId();
        await _service.DeactivateAsync(userId, id, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = UserId();
        await _service.SoftDeleteAsync(userId, id, ct);
        return NoContent();
    }

    private Guid UserId()
    {
        var id = User.FindFirst(AuthClaims.UserId)?.Value;
        if (id is null) throw new InvalidOperationException("Missing uid claim.");
        return Guid.Parse(id);
    }
}