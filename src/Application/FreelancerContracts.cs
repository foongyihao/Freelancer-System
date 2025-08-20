using CDN.Freelancers.Domain;

namespace CDN.Freelancers.Application;

/// <summary>
/// Defines methods for managing <see cref="Freelancer"/> entities in a repository.
/// </summary>
public interface IFreelancerRepository
{
    Task<Freelancer?> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Freelancer freelancer, CancellationToken ct = default);
    Task UpdateAsync(Freelancer freelancer, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ArchiveAsync(Guid id, bool archive, CancellationToken ct = default);
    Task<PaginatedResult<Freelancer>> GetPagedAsync(int page, int pageSize, bool includeArchived, string? skillFilter = null, string? hobbyFilter = null, string? term = null, CancellationToken ct = default);
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

/// <summary>
/// Exception thrown when attempting to create or update a freelancer with a duplicate username or email.
/// </summary>
public sealed class DuplicateFreelancerException : Exception
{
    public DuplicateFreelancerException(string message) : base(message) {}
}
