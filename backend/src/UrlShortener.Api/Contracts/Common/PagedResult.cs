namespace UrlShortener.Api.Contracts.Common;

/// <summary>
/// Пагінована відповідь зі списком елементів.
/// </summary>
/// <typeparam name="T">Тип елемента у списку.</typeparam>
/// <param name="Items">Елементи поточної сторінки.</param>
/// <param name="Page">Номер сторінки.</param>
/// <param name="PageSize">Розмір сторінки.</param>
/// <param name="Total">Загальна кількість елементів.</param>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long Total);