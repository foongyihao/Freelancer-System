using CDN.Freelancers.Core;

namespace CDN.Freelancers.Application;

/// <summary>
/// Defines methods for managing <see cref="Freelancer"/> entities in a repository.
/// </summary>
public interface IFreelancerRepository
{
    /// <summary>
    /// Retrieves a freelancer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the freelancer.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>The freelancer if found; otherwise, null.</returns>
    Task<Freelancer?> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all freelancers, optionally including archived ones.
    /// </summary>
    /// <param name="includeArchived">Whether to include archived freelancers.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A read-only list of freelancers.</returns>
    Task<IReadOnlyList<Freelancer>> GetAllAsync(bool includeArchived, CancellationToken ct = default);

    /// <summary>
    /// Adds a new freelancer to the repository.
    /// </summary>
    /// <param name="freelancer">The freelancer to add.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task AddAsync(Freelancer freelancer, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing freelancer in the repository.
    /// </summary>
    /// <param name="freelancer">The freelancer to update.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task UpdateAsync(Freelancer freelancer, CancellationToken ct = default);

    /// <summary>
    /// Deletes a freelancer from the repository by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the freelancer to delete.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Searches for freelancers matching the specified term.
    /// </summary>
    /// <param name="term">The search term.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A read-only list of matching freelancers.</returns>
    Task<IReadOnlyList<Freelancer>> SearchAsync(string term, CancellationToken ct = default);

    /// <summary>
    /// Archives or unarchives a freelancer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the freelancer.</param>
    /// <param name="archive">True to archive; false to unarchive.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task ArchiveAsync(Guid id, bool archive, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a page of freelancers (ordered by Username ascending).
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Page size (max 100).</param>
    /// <param name="includeArchived">Whether to include archived freelancers.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task<PaginatedResult<Freelancer>> GetPagedAsync(int page, int pageSize, bool includeArchived, CancellationToken ct = default);

    /// <summary>
    /// Performs a paginated search over Username & Email (case-insensitive substring match).
    /// </summary>
    /// <param name="term">Search substring.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Page size (max 100).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<PaginatedResult<Freelancer>> SearchPagedAsync(string term, int page, int pageSize, CancellationToken ct = default);
}

/// <summary>
/// Standard wrapper for paginated results.
/// </summary>
public sealed class PaginatedResult<T>
{
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
