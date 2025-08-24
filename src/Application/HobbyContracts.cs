using CDN.Freelancers.Domain;

namespace CDN.Freelancers.Application;

/// <summary>
/// Defines methods for managing <see cref="Hobby"/> master entities.
/// </summary>
public interface IHobbyRepository
{
    Task<Hobby?> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Hobby hobby, CancellationToken ct = default);
    Task UpdateAsync(Guid id, string name, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<PaginatedResult<Hobby>> GetPagedAsync(int page, int pageSize, string? term = null, CancellationToken ct = default);
}

/// <summary>
/// Exception thrown when attempting to create or rename a hobby with a duplicate name.
/// </summary>
public sealed class DuplicateHobbyException : Exception
{
    public DuplicateHobbyException(string message) : base(message) {}
}
