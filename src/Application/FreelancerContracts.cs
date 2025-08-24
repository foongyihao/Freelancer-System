using CDN.Freelancers.Domain;

namespace CDN.Freelancers.Application;

/// <summary>
/// Defines methods for managing <see cref="Freelancer"/> entities in a repository.
/// </summary>
public interface IFreelancerRepository {
    Task<Freelancer?> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Freelancer freelancer, CancellationToken ct = default);
    Task UpdateAsync(Freelancer freelancer, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ArchiveAsync(Guid id, bool archive, CancellationToken ct = default);
    Task<PaginatedResult<Freelancer>> GetPagedAsync(int page, int pageSize, bool includeArchived, string? skillFilter = null, string? hobbyFilter = null, string? term = null, CancellationToken ct = default);
}