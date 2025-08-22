using CDN.Freelancers.Domain;
using CDN.Freelancers.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CDN.Freelancers.Presentation.Controllers;

[ApiController]
[Route("api/v1/skills")]
public class SkillsetController : ControllerBase
{
    private readonly FreelancerDbContext _ctx;
    public SkillsetController(FreelancerDbContext ctx) { _ctx = ctx; }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? term = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);
        var q = _ctx.Skillsets.AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var t = term.ToLower();
            q = q.Where(s => s.Name.ToLower().Contains(t));
        }
        q = q.OrderBy(s => s.Name);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new {
            totalCount = total,
            page,
            pageSize,
            totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
            items = items.Select(s => new { id = s.Id, name = s.Name })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NameDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Name)) return Problem("Name is required", statusCode:400);
        var exists = await _ctx.Skillsets.AnyAsync(s => s.Name.ToLower() == dto.Name.ToLower());
        if (exists) return Problem("Skill already exists", statusCode:409);
        var s = new Skillset { Name = dto.Name.Trim() };
        _ctx.Skillsets.Add(s);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { term = s.Name }, new { id = s.Id, name = s.Name });
    }

    public record NameDto(string Name);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] NameDto dto)
    {
        var name = dto?.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return Problem("Name is required", statusCode:400);
        var entity = await _ctx.Skillsets.FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) return NotFound();
        var dup = await _ctx.Skillsets.AnyAsync(s => s.Id != id && s.Name.ToLower() == name.ToLower());
        if (dup) return Problem("Skill already exists", statusCode:409);
        entity.Name = name;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _ctx.Skillsets.FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) return NotFound();
        _ctx.Skillsets.Remove(entity);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}
