using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    /// <summary>
    /// Створює нове коротке посилання для поточного користувача.
    /// </summary>
    /// <param name="request">Параметри для створення короткого посилання.</param>
    /// <param name="ct">Токен скасування запиту.</param>
    /// <response code="201">Коротке посилання успішно створено.</response>
    [HttpPost]
    public async Task<ActionResult<ShortUrlDto>> Create(CreateShortUrlRequest request, CancellationToken ct)
    {
        var userId = UserId();
        var created = await _service.CreateAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Повертає пагінований список коротких посилань поточного користувача.
    /// </summary>
    /// <param name="page">Номер сторінки.</param>
    /// <param name="pageSize">Розмір сторінки.</param>
    /// <param name="ct">Токен скасування запиту.</param>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ShortUrlDto>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userId = UserId();
        var result = await _service.ListAsync(userId, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Повертає коротке посилання за ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор короткого посилання.</param>
    /// <param name="ct">Токен скасування запиту.</param>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShortUrlDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = UserId();
        return Ok(await _service.GetAsync(userId, id, ct));
    }

    /// <summary>
    /// Деактивує коротке посилання.
    /// </summary>
    /// <param name="id">Ідентифікатор короткого посилання.</param>
    /// <param name="ct">Токен скасування запиту.</param>
    /// <response code="204">Посилання деактивовано.</response>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var userId = UserId();
        await _service.DeactivateAsync(userId, id, ct);
        return NoContent();
    }

    /// <summary>
    /// М'яко видаляє коротке посилання.
    /// </summary>
    /// <param name="id">Ідентифікатор короткого посилання.</param>
    /// <param name="ct">Токен скасування запиту.</param>
    /// <response code="204">Посилання позначено як видалене.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = UserId();
        await _service.SoftDeleteAsync(userId, id, ct);
        return NoContent();
    }

    /// <summary>
    /// Повертає статистику переходів за період.
    /// </summary>
    /// <param name="id">Ідентифікатор короткого посилання.</param>
    /// <param name="from">Початок періоду (включно).</param>
    /// <param name="to">Кінець періоду (включно).</param>
    /// <param name="ct">Токен скасування запиту.</param>
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

    /// <summary>
    /// Повертає деталізацію переходів за країнами, пристроями, браузерами та ОС.
    /// </summary>
    /// <param name="id">Ідентифікатор короткого посилання.</param>
    /// <param name="from">Початок періоду (включно).</param>
    /// <param name="to">Кінець періоду (включно).</param>
    /// <param name="ct">Токен скасування запиту.</param>
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

    /// <summary>
    /// Повертає останні зафіксовані переходи за коротким посиланням.
    /// </summary>
    /// <param name="id">Ідентифікатор короткого посилання.</param>
    /// <param name="take">Кількість останніх подій.</param>
    /// <param name="ct">Токен скасування запиту.</param>
    [HttpGet("{id:guid}/clicks")]
    public async Task<ActionResult<IReadOnlyList<ClickEventDto>>> RecentClicks(
        Guid id,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var userId = UserId();
        return Ok(await _analytics.GetRecentClicksAsync(userId, id, take, ct));
    }

}