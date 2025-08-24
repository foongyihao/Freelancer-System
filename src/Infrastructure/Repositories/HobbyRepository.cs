using CDN.Freelancers.Application;
using CDN.Freelancers.Domain;
using Microsoft.EntityFrameworkCore;
using CDN.Freelancers.Domain.Exceptions;

namespace CDN.Freelancers.Infrastructure;

/// <summary>
/// EF Core repository for <see cref="Hobby"/> master data.
/// </summary>
public class HobbyRepository : IHobbyRepository {
    private readonly FreelancerDbContext _ctx;
    public HobbyRepository(FreelancerDbContext ctx) => _ctx = ctx;

    /// <inheritdoc />
    public async Task AddAsync(Hobby hobby, CancellationToken ct = default) {
        var name = (hobby.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(hobby));
        // Check duplicate by Id or Name
        if (hobby.Id != Guid.Empty)
        {
            var idExists = await _ctx.Hobbies.AnyAsync(s => s.Id == hobby.Id, ct);
            if (idExists) throw new DuplicateRecordException(nameof(Hobby), hobby.Id);
        }
        var dup = await _ctx.Hobbies.AnyAsync(s => s.Name.ToLower() == name.ToLower(), ct);
        if (dup) throw new DuplicateRecordException(nameof(Hobby), name, "Hobby name already exists.");
        hobby.Id = hobby.Id == Guid.Empty ? Guid.NewGuid() : hobby.Id;
        hobby.Name = name;
        _ctx.Hobbies.Add(hobby);
        await _ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken ct = default) {
        var entity = await _ctx.Hobbies.FindAsync(new object?[] { id }, ct);
        if (entity == null) throw new EntityNotFoundException(nameof(Hobby), id);
        _ctx.Hobbies.Remove(entity);
        await _ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Hobby?> GetAsync(Guid id, CancellationToken ct = default) {
        return await _ctx.Hobbies.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<Hobby>> GetPagedAsync(int page, int pageSize, string? term = null, CancellationToken ct = default) {
        page = page < 1 ? 1 : page;
        if (pageSize <= 0) pageSize = 10;
        pageSize = pageSize > 100 ? 100 : pageSize;
        var q = _ctx.Hobbies.AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var t = term.ToLower();
            q = q.Where(s => s.Name.ToLower().Contains(t));
        }
        var ordered = q.OrderBy(s => s.Name);
        var total = await ordered.CountAsync(ct);
        var items = await ordered.Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return new PaginatedResult<Hobby>{ TotalCount = total, Page = page, PageSize = pageSize, Items = items };
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Guid id, string name, CancellationToken ct = default) {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
    var entity = await _ctx.Hobbies.FirstOrDefaultAsync(s => s.Id == id, ct);
    if (entity == null) throw new EntityNotFoundException(nameof(Hobby), id);
    var dup = await _ctx.Hobbies.AnyAsync(s => s.Id != id && s.Name.ToLower() == name.ToLower(), ct);
    if (dup) throw new DuplicateRecordException(nameof(Hobby), name, "Hobby name already exists.");
        entity.Name = name;
        await _ctx.SaveChangesAsync(ct);
    }
}
