using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using CDN.Freelancers.Application;
using CDN.Freelancers.Domain;
using CDN.Freelancers.Presentation.Requests;

namespace CDN.Freelancers.Presentation.Controllers;

[ApiController]
[Route("api/v1/freelancers")]
[Produces("application/json")]
public class FreelancersController : ControllerBase
{
    private readonly IFreelancerRepository _repo;
    public FreelancersController(IFreelancerRepository repo) => _repo = repo;

    /// <summary>
    /// List freelancers with pagination (non-archived by default).
    /// </summary>
    /// <param name="term">Optional case-insensitive substring to match username or email.</param>
    /// <param name="includeArchived">Includes freelancers who are archived when true.</param>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Page size (default 10, max 100).</param>
    /// <param name="skill">Optional case-insensitive substring to match a skill name.</param>
    /// <param name="hobby">Optional case-insensitive substring to match a hobby name.</param>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<Freelancer>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(
            [FromQuery, Description("Case-insensitive username/email contains filter (optional). ")] string? term = null,
            [FromQuery, Description("Includes archived when true.")] bool includeArchived = false,
            [FromQuery, Description("1-based page number (default 1)." )] int page = 1,
            [FromQuery, Description("Page size (default 10, max 100)." )] int pageSize = 10,
            [FromQuery, Description("Filter containing skill substring (optional)." )] string? skill = null,
            [FromQuery, Description("Filter containing hobby substring (optional)." )] string? hobby = null)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return Problem(
                    title: "Invalid paging parameters",
                    detail: "'page' must be >= 1 and 'pageSize' must be between 1 and 100.",
                    statusCode: StatusCodes.Status400BadRequest);
            if (!string.IsNullOrWhiteSpace(term))
                return Ok(await _repo.SearchPagedAsync(term, page, pageSize, skill, hobby));
            return Ok(await _repo.GetPagedAsync(page, pageSize, includeArchived, skill, hobby));
        }

    /// <summary>
    /// Get a single freelancer by Id.
    /// </summary>
    /// <param name="id">Freelancer GUID.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Freelancer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOne(Guid id)
    {
        var f = await _repo.GetAsync(id);
        return f is null ? NotFound() : Ok(f);
    }

    // Removed separate /search endpoint: unified via optional 'term' in GET

    /// <summary>
    /// Create a new freelancer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Freelancer), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] FreelancerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
            return Problem(title: "Validation error", detail: "Username and Email are required.", statusCode: StatusCodes.Status400BadRequest);
        if (!request.Email.Contains('@'))
            return Problem(title: "Validation error", detail: "Email must contain '@'.", statusCode: StatusCodes.Status400BadRequest);
        var model = new Freelancer
        {
            Username = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber ?? string.Empty,
            Skillsets = (request.Skillsets ?? new List<string>()).Select(s => new Skillset { Name = s }).ToList(),
            Hobbies = (request.Hobbies ?? new List<string>()).Select(h => new Hobby { Name = h }).ToList()
        };
    model.IsArchived = request.IsArchived;
        try
        {
            await _repo.AddAsync(model);
        }
        catch (DuplicateFreelancerException ex)
        {
            return Problem(title: "Duplicate freelancer", detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
        return CreatedAtAction(nameof(GetOne), new { id = model.Id }, model);
    }

    /// <summary>
    /// Replace an existing freelancer and its skillsets/hobbies collections.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] FreelancerRequest request)
    {
        var existing = await _repo.GetAsync(id);
        if (existing == null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
            return Problem(title: "Validation error", detail: "Username and Email are required.", statusCode: StatusCodes.Status400BadRequest);
        if (!request.Email.Contains('@'))
            return Problem(title: "Validation error", detail: "Email must contain '@'.", statusCode: StatusCodes.Status400BadRequest);
        var model = new Freelancer
        {
            Id = id,
            Username = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber ?? string.Empty,
            Skillsets = (request.Skillsets ?? new List<string>()).Select(s => new Skillset { Name = s, FreelancerId = id }).ToList(),
            Hobbies = (request.Hobbies ?? new List<string>()).Select(h => new Hobby { Name = h, FreelancerId = id }).ToList()
        };
    model.IsArchived = request.IsArchived;
        try
        {
            await _repo.UpdateAsync(model);
        }
        catch (DuplicateFreelancerException ex)
        {
            return Problem(title: "Duplicate freelancer", detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
        return NoContent();
    }

    /// <summary>
    /// Partial update (currently only supports toggling archive state).
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(Guid id, [FromBody] FreelancerPatchRequest request)
    {
        var existing = await _repo.GetAsync(id);
        if (existing == null) return NotFound();
        if (request?.IsArchived is null)
        {
            return Problem(title: "Invalid patch payload", detail: "Provide 'isArchived' boolean field.", statusCode: StatusCodes.Status400BadRequest);
        }
        await _repo.ArchiveAsync(id, request.IsArchived.Value);
        return NoContent();
    }

    /// <summary>
    /// Delete a freelancer and its related collections.
    /// </summary>
    /// <param name="id">Freelancer GUID.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
    var existing = await _repo.GetAsync(id);
    if (existing == null) return NotFound();
    await _repo.DeleteAsync(id);
    return NoContent();
    }
}
