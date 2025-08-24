using CDN.Freelancers.Domain;

namespace CDN.Freelancers.Application;

/// <summary>
/// Defines methods for managing <see cref="Skillset"/> master entities.
/// </summary>
public interface ISkillsetRepository
{
    Task<Skillset?> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Skillset skillset, CancellationToken ct = default);
    Task UpdateAsync(Guid id, string name, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<PaginatedResult<Skillset>> GetPagedAsync(int page, int pageSize, string? term = null, CancellationToken ct = default);
}

/// <summary>
/// Exception thrown when attempting to create or rename a skill with a duplicate name.
/// </summary>
public sealed class DuplicateSkillsetException : Exception
{
    public DuplicateSkillsetException(string message) : base(message) {}
}
