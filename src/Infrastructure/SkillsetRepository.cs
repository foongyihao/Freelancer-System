using CDN.Freelancers.Application;
using CDN.Freelancers.Domain;
using Microsoft.EntityFrameworkCore;
using CDN.Freelancers.Domain.Exceptions;

namespace CDN.Freelancers.Infrastructure;

/// <summary>
/// EF Core repository for <see cref="Skillset"/> master data.
/// </summary>
public class SkillsetRepository : ISkillsetRepository {
    private readonly FreelancerDbContext _ctx;
    public SkillsetRepository(FreelancerDbContext ctx) => _ctx = ctx;

    /// <inheritdoc />
    public async Task AddAsync(Skillset skillset, CancellationToken ct = default) {
        var name = (skillset.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(skillset));
        // Check duplicate by Id or Name
        if (skillset.Id != Guid.Empty)
        {
            var idExists = await _ctx.Skillsets.AnyAsync(s => s.Id == skillset.Id, ct);
            if (idExists) throw new DuplicateRecordException(nameof(Skillset), skillset.Id);
        }
        var dup = await _ctx.Skillsets.AnyAsync(s => s.Name.ToLower() == name.ToLower(), ct);
        if (dup) throw new DuplicateRecordException(nameof(Skillset), name, "Skill name already exists.");
        skillset.Id = skillset.Id == Guid.Empty ? Guid.NewGuid() : skillset.Id;
        skillset.Name = name;
        _ctx.Skillsets.Add(skillset);
        await _ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken ct = default) {
        var entity = await _ctx.Skillsets.FindAsync(new object?[] { id }, ct);
        if (entity == null) throw new EntityNotFoundException(nameof(Skillset), id);
        _ctx.Skillsets.Remove(entity);
        await _ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Skillset?> GetAsync(Guid id, CancellationToken ct = default) {
        return await _ctx.Skillsets.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<Skillset>> GetPagedAsync(int page, int pageSize, string? term = null, CancellationToken ct = default) {
        page = page < 1 ? 1 : page;
        if (pageSize <= 0) pageSize = 10;
        pageSize = pageSize > 100 ? 100 : pageSize;
        var q = _ctx.Skillsets.AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var t = term.ToLower();
            q = q.Where(s => s.Name.ToLower().Contains(t));
        }
        var ordered = q.OrderBy(s => s.Name);
        var total = await ordered.CountAsync(ct);
        var items = await ordered.Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return new PaginatedResult<Skillset>{ TotalCount = total, Page = page, PageSize = pageSize, Items = items };
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Guid id, string name, CancellationToken ct = default) {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
    var entity = await _ctx.Skillsets.FirstOrDefaultAsync(s => s.Id == id, ct);
    if (entity == null) throw new EntityNotFoundException(nameof(Skillset), id);
    var dup = await _ctx.Skillsets.AnyAsync(s => s.Id != id && s.Name.ToLower() == name.ToLower(), ct);
    if (dup) throw new DuplicateRecordException(nameof(Skillset), name, "Skill name already exists.");
        entity.Name = name;
        await _ctx.SaveChangesAsync(ct);
    }
}
