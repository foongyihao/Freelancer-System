using CDN.Freelancers.Domain;
using CDN.Freelancers.Application;
using CDN.Freelancers.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CDN.Freelancers.Presentation.Requests;

namespace CDN.Freelancers.Presentation.Controllers;

/// <summary>
/// Represents hobby to be associated with freelancers.
/// Hobbies are master entities (e.g., "Chess", "Photography") that can be
/// linked to freelancers in a many-to-many relationship.
/// </summary>
[ApiController]
[Route("api/v1/hobbies")]
public class HobbyController : ControllerBase {
    private readonly FreelancerDbContext _ctx;
    public HobbyController(FreelancerDbContext ctx) { _ctx = ctx; }

    /// <summary>
    /// Returns a paged list of hobbies with optional case-insensitive term filter.
    /// </summary>
    /// <param name="term">Optional case-insensitive substring to search by name.</param>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Page size (default 10, max 100).</param>
    /// <returns>Metadata and items: id and name.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<Hobby>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromQuery] string? term = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);
        var q = _ctx.Hobbies.AsQueryable();
        if (!string.IsNullOrWhiteSpace(term)) {
            var t = term.ToLower();
            q = q.Where(h => h.Name.ToLower().Contains(t));
        }
        q = q.OrderBy(h => h.Name);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new {
            totalCount = total,
            page,
            pageSize,
            totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
            items = items.Select(h => new { id = h.Id, name = h.Name })
        });
    }

    /// <summary>
    /// Creates a new hobby entry with a unique name.
    /// </summary>
    /// <param name="dto">Request with the hobby Name.</param>
    /// <returns>201 Created with the created resource shape.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Hobby), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] HobbyRequest dto) {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Name)) return Problem("Name is required", statusCode:400);
        var exists = await _ctx.Hobbies.AnyAsync(h => h.Name.ToLower() == dto.Name.ToLower());
        if (exists) return Problem("Hobby already exists", statusCode:409);
        var h = new Hobby { Name = dto.Name.Trim() };
        _ctx.Hobbies.Add(h);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { term = h.Name }, new { id = h.Id, name = h.Name });
    }

    /// <summary>
    /// Updates a hobby name by id.
    /// </summary>
    /// <param name="id">Hobby id.</param>
    /// <param name="dto">New name payload.</param>
    /// <returns>204 No Content on success, 404 if not found, 409 on duplicate.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] HobbyRequest dto) {
        var name = dto?.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return Problem("Name is required", statusCode:400);
        var entity = await _ctx.Hobbies.FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) return NotFound();
        var dup = await _ctx.Hobbies.AnyAsync(s => s.Id != id && s.Name.ToLower() == name.ToLower());
        if (dup) return Problem("Hobby already exists", statusCode:409);
        entity.Name = name;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes a hobby by id.
    /// </summary>
    /// <param name="id">Hobby id.</param>
    /// <returns>204 No Content on success or 404 if not found.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id) {
        var entity = await _ctx.Hobbies.FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) return NotFound();
        _ctx.Hobbies.Remove(entity);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}
