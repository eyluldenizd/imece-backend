namespace Application.DTOs;

/// <summary>
/// Ortak sunucu-taraflı sayfalama cevabı.
/// </summary>
public sealed class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }
}
