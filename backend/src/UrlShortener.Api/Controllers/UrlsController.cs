using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Common.Auth;
using UrlShortener.Api.Contracts.Common;
using UrlShortener.Api.Contracts.Analytics;
using UrlShortener.Api.Contracts.Urls;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/urls")]
public sealed class UrlsController : BaseApiController
{
    private readonly ShortUrlService _service;
    private readonly AnalyticsService _analytics;

    public UrlsController(ShortUrlService service, AnalyticsService analytics)
    {
        _service = service;
        _analytics = analytics;
    }

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

    [HttpGet("{id:guid}/stats")]
    public async Task<ActionResult<UrlStatsResponse>> Stats(
        Guid id,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var userId = UserId();

        // Defaults: last 14 days
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromEff = from ?? today.AddDays(-13);
        var toEff = to ?? today;

        return Ok(await _analytics.GetStatsAsync(userId, id, fromEff, toEff, ct));
    }

    [HttpGet("{id:guid}/breakdown")]
    public async Task<ActionResult<UrlBreakdownResponse>> Breakdown(
        Guid id,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var userId = UserId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromEff = from ?? today.AddDays(-13);
        var toEff = to ?? today;

        return Ok(await _analytics.GetBreakdownAsync(userId, id, fromEff, toEff, ct));
    }

    [HttpGet("{id:guid}/clicks")]
    public async Task<ActionResult<IReadOnlyList<ClickEventDto>>> RecentClicks(
        Guid id,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var userId = UserId();
        return Ok(await _analytics.GetRecentClicksAsync(userId, id, take, ct));
    }

    private Guid UserId()
    {
        var id = User.FindFirst(AuthClaims.UserId)?.Value;
        if (id is null) throw new InvalidOperationException("Missing uid claim.");
        return Guid.Parse(id);
    }
}