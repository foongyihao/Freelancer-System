namespace CDN.Freelancers.Application;

/// <summary>
/// Standard wrapper for paginated results.
/// </summary>
public sealed class PaginatedResult<T> {
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalPages => PageSize == 0 ? 0 : (int)System.Math.Ceiling((double)TotalCount / PageSize);
}
