namespace PotatoAccounting.Application.Common.Models;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public static class PagingDefaults
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int page, int pageSize) Normalize(int? page, int? pageSize)
    {
        var p = page is > 0 ? page.Value : DefaultPage;
        var s = pageSize is > 0 ? Math.Min(pageSize.Value, MaxPageSize) : DefaultPageSize;
        return (p, s);
    }
}
