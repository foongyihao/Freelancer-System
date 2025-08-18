using CDN.Freelancers.Application;
using CDN.Freelancers.Core;
using Microsoft.EntityFrameworkCore;

namespace CDN.Freelancers.Infrastructure;

/// <summary>
/// Entity Framework Core implementation of <see cref="IFreelancerRepository"/> for persisting
/// and querying <see cref="Freelancer"/> aggregates including their child collections
/// (<see cref="Skillset"/> and <see cref="Hobby"/>).
/// </summary>
/// <remarks>
/// This repository keeps the implementation intentionally straightforward for assessment purposes.
/// Concurrency handling, partial updates (patch semantics), domain events and validation are
/// intentionally omitted but could be added in a production system.
/// </remarks>
public class FreelancerRepository : IFreelancerRepository
{
    private readonly FreelancerDbContext _ctx;
    /// <summary>
    /// Creates a repository instance bound to a specific <see cref="FreelancerDbContext"/>.
    /// </summary>
    public FreelancerRepository(FreelancerDbContext ctx) => _ctx = ctx;

    /// <inheritdoc />
    public async Task AddAsync(Freelancer freelancer, CancellationToken ct = default)
    {
        _ctx.Freelancers.Add(freelancer);
        await _ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task ArchiveAsync(Guid id, bool archive, CancellationToken ct = default)
    {
        var entity = await _ctx.Freelancers.FindAsync(new object?[] { id }, ct);
        if (entity == null) return;
        entity.IsArchived = archive;
        await _ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _ctx.Freelancers.Include(f=>f.Skillsets).Include(f=>f.Hobbies).FirstOrDefaultAsync(f=>f.Id==id, ct);
        if (entity == null) return;
        _ctx.Remove(entity);
        await _ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Freelancer>> GetAllAsync(bool includeArchived, CancellationToken ct = default)
    {
        var q = _ctx.Freelancers
            .Include(f => f.Skillsets)
            .Include(f => f.Hobbies)
            .AsQueryable();
        if (!includeArchived) q = q.Where(f => !f.IsArchived);
        return await q.OrderBy(f=>f.Username).ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Freelancer?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _ctx.Freelancers.Include(f=>f.Skillsets).Include(f=>f.Hobbies).FirstOrDefaultAsync(f=>f.Id==id, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Freelancer>> SearchAsync(string term, CancellationToken ct = default)
    {
        term = term.ToLower();
        return await _ctx.Freelancers
            .Include(f=>f.Skillsets).Include(f=>f.Hobbies)
            .Where(f => f.Username.ToLower().Contains(term) || f.Email.ToLower().Contains(term))
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Freelancer freelancer, CancellationToken ct = default)
    {
        var existing = await _ctx.Freelancers.Include(f=>f.Skillsets).Include(f=>f.Hobbies).FirstOrDefaultAsync(f=>f.Id==freelancer.Id, ct);
        if (existing == null) return;
        existing.Username = freelancer.Username;
        existing.Email = freelancer.Email;
        existing.PhoneNumber = freelancer.PhoneNumber;
        existing.IsArchived = freelancer.IsArchived;
        // Replace skillsets & hobbies simplistic approach
        _ctx.Skillsets.RemoveRange(existing.Skillsets);
        _ctx.Hobbies.RemoveRange(existing.Hobbies);
        existing.Skillsets = freelancer.Skillsets;
        existing.Hobbies = freelancer.Hobbies;
        await _ctx.SaveChangesAsync(ct);
    }
}
