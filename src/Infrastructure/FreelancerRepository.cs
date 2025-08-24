using CDN.Freelancers.Application;
using CDN.Freelancers.Domain;
using CDN.Freelancers.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CDN.Freelancers.Infrastructure;

/// <summary>
/// Entity Framework Core implementation of <see cref="IFreelancerRepository"/> for persisting
/// and querying <see cref="Freelancer"/> aggregates including their child collections
/// (<see cref="Skillset"/> and <see cref="Hobby"/>).
/// </summary>
public class FreelancerRepository : IFreelancerRepository {
    private readonly FreelancerDbContext _ctx;
    /// <summary>
    /// Creates a repository instance bound to a specific <see cref="FreelancerDbContext"/>.
    /// </summary>
    public FreelancerRepository(FreelancerDbContext ctx) => _ctx = ctx;

    /// <inheritdoc />
    public async Task AddAsync(Freelancer freelancer, CancellationToken ct = default) {
        // Duplicate check (username/email uniqueness)
    var exists = await _ctx.Freelancers.AnyAsync(f => f.Username == freelancer.Username || f.Email == freelancer.Email, ct);
    if (exists) throw new DuplicateRecordException(nameof(Freelancer), $"{freelancer.Username}/{freelancer.Email}", "Username or Email already exists.");

        freelancer.PhoneNumber ??= string.Empty;
        if (freelancer.Id == Guid.Empty) freelancer.Id = Guid.NewGuid();

        // Resolve join collections: prefer ids if they were mapped onto the aggregate; otherwise use names
        List<Skillset> skills;
        List<Hobby> hobbies;
        var desiredSkillIds = (freelancer.FreelancerSkillsets ?? new()).Select(x => x.SkillsetId).Where(id => id != Guid.Empty).Distinct().ToList();
        var desiredHobbyIds = (freelancer.FreelancerHobbies ?? new()).Select(x => x.HobbyId).Where(id => id != Guid.Empty).Distinct().ToList();
        if (desiredSkillIds.Count > 0) {
            skills = await _ctx.Skillsets.Where(s => desiredSkillIds.Contains(s.Id)).ToListAsync(ct);
        }
        else {
            var desiredSkillNames = (freelancer.FreelancerSkillsets ?? new()).Select(x => x.Skillset?.Name?.Trim() ?? string.Empty)
                .Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            skills = await ResolveSkillsAsync(desiredSkillNames, ct);
        }
        if (desiredHobbyIds.Count > 0) {
            hobbies = await _ctx.Hobbies.Where(h => desiredHobbyIds.Contains(h.Id)).ToListAsync(ct);
        }
        else {
            var desiredHobbyNames = (freelancer.FreelancerHobbies ?? new()).Select(x => x.Hobby?.Name?.Trim() ?? string.Empty)
                .Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            hobbies = await ResolveHobbiesAsync(desiredHobbyNames, ct);
        }

        freelancer.FreelancerSkillsets = skills.Select(s => new Freelancer_Skillset { FreelancerId = freelancer.Id, SkillsetId = s.Id }).ToList();
        freelancer.FreelancerHobbies = hobbies.Select(h => new Freelancer_Hobby { FreelancerId = freelancer.Id, HobbyId = h.Id }).ToList();

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
        var entity = await _ctx.Freelancers.FirstOrDefaultAsync(f=>f.Id==id, ct);
        if (entity == null) return;
        _ctx.Remove(entity);
        await _ctx.SaveChangesAsync(ct);
    }
    
    /// <inheritdoc />
    public async Task<Freelancer?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _ctx.Freelancers
            .Include(f => f.FreelancerSkillsets).ThenInclude(fs => fs.Skillset)
            .Include(f => f.FreelancerHobbies).ThenInclude(fh => fh.Hobby)
            .FirstOrDefaultAsync(f=>f.Id==id, ct);
    }
    
    /// <inheritdoc />
    public async Task UpdateAsync(Freelancer freelancer, CancellationToken ct = default)
    {
        var existing = await _ctx.Freelancers
            .Include(f => f.FreelancerSkillsets)
            .Include(f => f.FreelancerHobbies)
            .FirstOrDefaultAsync(f=>f.Id==freelancer.Id, ct);
        if (existing == null) return;

    var duplicate = await _ctx.Freelancers.AnyAsync(f => f.Id != freelancer.Id && (f.Username == freelancer.Username || f.Email == freelancer.Email), ct);
    if (duplicate) throw new DuplicateRecordException(nameof(Freelancer), $"{freelancer.Username}/{freelancer.Email}", "Username or Email already exists.");

        existing.Username = freelancer.Username;
        existing.Email = freelancer.Email;
        existing.PhoneNumber = freelancer.PhoneNumber ?? string.Empty;
        existing.IsArchived = freelancer.IsArchived;

        // Resolve desired from ids first, else names
        List<Skillset> skills;
        List<Hobby> hobbies;
        var desiredSkillIds = (freelancer.FreelancerSkillsets ?? new()).Select(x => x.SkillsetId).Where(id => id != Guid.Empty).Distinct().ToList();
        var desiredHobbyIds = (freelancer.FreelancerHobbies ?? new()).Select(x => x.HobbyId).Where(id => id != Guid.Empty).Distinct().ToList();
        if (desiredSkillIds.Count > 0)
        {
            skills = await _ctx.Skillsets.Where(s => desiredSkillIds.Contains(s.Id)).ToListAsync(ct);
        }
        else
        {
            var desiredSkillNames = (freelancer.FreelancerSkillsets ?? new()).Select(x => x.Skillset?.Name?.Trim() ?? string.Empty)
                .Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            skills = await ResolveSkillsAsync(desiredSkillNames, ct);
        }
        if (desiredHobbyIds.Count > 0)
        {
            hobbies = await _ctx.Hobbies.Where(h => desiredHobbyIds.Contains(h.Id)).ToListAsync(ct);
        }
        else
        {
            var desiredHobbyNames = (freelancer.FreelancerHobbies ?? new()).Select(x => x.Hobby?.Name?.Trim() ?? string.Empty)
                .Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            hobbies = await ResolveHobbiesAsync(desiredHobbyNames, ct);
        }

        // Replace join rows
        _ctx.FreelancerSkillsets.RemoveRange(existing.FreelancerSkillsets);
        _ctx.FreelancerHobbies.RemoveRange(existing.FreelancerHobbies);
        existing.FreelancerSkillsets = skills.Select(s => new Freelancer_Skillset { FreelancerId = existing.Id, SkillsetId = s.Id }).ToList();
        existing.FreelancerHobbies = hobbies.Select(h => new Freelancer_Hobby { FreelancerId = existing.Id, HobbyId = h.Id }).ToList();

        await _ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<Freelancer>> GetPagedAsync(int page, int pageSize, bool includeArchived, string? skillFilter = null, string? hobbyFilter = null, string? term = null, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        if (pageSize <= 0) pageSize = 10;
        pageSize = pageSize > 100 ? 100 : pageSize;
        var q = _ctx.Freelancers
            .Include(f => f.FreelancerSkillsets).ThenInclude(fs => fs.Skillset)
            .Include(f => f.FreelancerHobbies).ThenInclude(fh => fh.Hobby)
            .AsQueryable();
        if (!includeArchived) q = q.Where(f => !f.IsArchived);
        if (!string.IsNullOrWhiteSpace(term))
        {
            var lower = term.ToLower();
            q = q.Where(f => f.Username.ToLower().Contains(lower) || f.Email.ToLower().Contains(lower));
        }
        if (!string.IsNullOrWhiteSpace(skillFilter))
        {
            var tokens = skillFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .Select(x => x.ToLower()).ToList();
            if(tokens.Count == 1)
            {
                var s = tokens[0];
                q = q.Where(f => f.FreelancerSkillsets.Any(fs => EF.Functions.Like(fs.Skillset.Name.ToLower(), $"%{s}%")));
            }
            else if(tokens.Count > 1)
            {
                q = q.Where(f => f.FreelancerSkillsets.Any(fs => tokens.Contains(fs.Skillset.Name.ToLower())));
            }
        }
        if (!string.IsNullOrWhiteSpace(hobbyFilter))
        {
            var tokens = hobbyFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .Select(x => x.ToLower()).ToList();
            if(tokens.Count == 1)
            {
                var h = tokens[0];
                q = q.Where(f => f.FreelancerHobbies.Any(fh => EF.Functions.Like(fh.Hobby.Name.ToLower(), $"%{h}%")));
            }
            else if(tokens.Count > 1)
            {
                q = q.Where(f => f.FreelancerHobbies.Any(fh => tokens.Contains(fh.Hobby.Name.ToLower())));
            }
        }
        var ordered = q.OrderBy(f=>f.Username);
        var total = await ordered.CountAsync(ct);
        var items = await ordered.Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return new PaginatedResult<Freelancer>{ TotalCount = total, Page = page, PageSize = pageSize, Items = items };
    }

    private async Task<List<Skillset>> ResolveSkillsAsync(IEnumerable<string> names, CancellationToken ct)
    {
        var list = names.Select(n => n.Trim()).Where(n => n.Length>0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (list.Count == 0) return new List<Skillset>();
        var lower = list.Select(n => n.ToLower()).ToList();
        var existing = await _ctx.Skillsets.Where(s => lower.Contains(s.Name.ToLower())).ToListAsync(ct);
        var toAdd = list.Where(n => !existing.Any(e => e.Name.Equals(n, StringComparison.OrdinalIgnoreCase)))
                        .Select(n => new Skillset { Name = n }).ToList();
        if (toAdd.Count > 0)
        {
            _ctx.Skillsets.AddRange(toAdd);
            await _ctx.SaveChangesAsync(ct);
            existing.AddRange(toAdd);
        }
        return existing;
    }

    private async Task<List<Hobby>> ResolveHobbiesAsync(IEnumerable<string> names, CancellationToken ct)
    {
        var list = names.Select(n => n.Trim()).Where(n => n.Length>0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (list.Count == 0) return new List<Hobby>();
        var lower = list.Select(n => n.ToLower()).ToList();
        var existing = await _ctx.Hobbies.Where(h => lower.Contains(h.Name.ToLower())).ToListAsync(ct);
        var toAdd = list.Where(n => !existing.Any(e => e.Name.Equals(n, StringComparison.OrdinalIgnoreCase)))
                        .Select(n => new Hobby { Name = n }).ToList();
        if (toAdd.Count > 0)
        {
            _ctx.Hobbies.AddRange(toAdd);
            await _ctx.SaveChangesAsync(ct);
            existing.AddRange(toAdd);
        }
        return existing;
    }
}
