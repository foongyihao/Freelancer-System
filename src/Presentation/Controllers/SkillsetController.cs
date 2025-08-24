using CDN.Freelancers.Domain;
using CDN.Freelancers.Application;
using CDN.Freelancers.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CDN.Freelancers.Presentation.Requests;

namespace CDN.Freelancers.Presentation.Controllers;

/// <summary>
/// Represents a Skill to be associated with freelancers.
/// Skillsets are master entities (e.g., "C#", "SQL", "React") that can be
/// linked to freelancers in a many-to-many relationship.
/// </summary>
[ApiController]
[Route("api/v1/skills")]
public class SkillsetController : ControllerBase {
    private readonly FreelancerDbContext _ctx;
    public SkillsetController(FreelancerDbContext ctx) { _ctx = ctx; }

    /// <summary>
    /// Returns a paged list of skills with optional case-insensitive term filter.
    /// </summary>
    /// <param name="term">Optional case-insensitive substring to search by name.</param>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Page size (default 10, max 100).</param>
    /// <returns>Metadata and items: id and name.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<Skillset>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromQuery] string? term = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);
        var q = _ctx.Skillsets.AsQueryable();
        if (!string.IsNullOrWhiteSpace(term)) {
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

    /// <summary>
    /// Creates a new skill entry with a unique name.
    /// </summary>
    /// <param name="dto">Request with the skill Name.</param>
    /// <returns>201 Created with the created resource shape.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Skillset), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] SkillsetRequest dto) {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Name)) return Problem("Name is required", statusCode:400);
        var exists = await _ctx.Skillsets.AnyAsync(s => s.Name.ToLower() == dto.Name.ToLower());
        if (exists) return Problem("Skill already exists", statusCode:409);
        var s = new Skillset { Name = dto.Name.Trim() };
        _ctx.Skillsets.Add(s);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { term = s.Name }, new { id = s.Id, name = s.Name });
    }

    /// <summary>
    /// Updates a skill name by id.
    /// </summary>
    /// <param name="id">Skill id.</param>
    /// <param name="dto">New name payload.</param>
    /// <returns>204 No Content on success, 404 if not found, 409 on duplicate.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SkillsetRequest dto) {
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

    /// <summary>
    /// Deletes a skill by id.
    /// </summary>
    /// <param name="id">Skill id.</param>
    /// <returns>204 No Content on success or 404 if not found.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id) {
        var entity = await _ctx.Skillsets.FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) return NotFound();
        _ctx.Skillsets.Remove(entity);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}
