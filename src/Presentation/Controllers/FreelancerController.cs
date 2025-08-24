using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using CDN.Freelancers.Application;
using CDN.Freelancers.Domain;
using CDN.Freelancers.Domain.Exceptions;
using CDN.Freelancers.Presentation.Requests;
using CDN.Freelancers.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CDN.Freelancers.Presentation.Controllers;

/// <summary>
/// Represents a freelancer record in the system.
/// </summary>
[ApiController]
[Route("api/v1/freelancers")]
[Produces("application/json")]
public class FreelancerController : ControllerBase {
    private readonly IFreelancerRepository _repo;
    private readonly FreelancerDbContext _ctx;
    public FreelancerController(IFreelancerRepository repo, FreelancerDbContext ctx) { _repo = repo; _ctx = ctx; }

    /// <summary>
    /// Validates the <see cref="FreelancerRequest"/> for required fields.
    /// Returns a <see cref="ProblemDetails"/> IActionResult if validation fails, otherwise null.
    /// </summary>
    /// <param name="request">The freelancer request to validate.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> with validation error details, or null if valid.
    /// </returns>
    private IActionResult? ValidateFreelancerRequest(FreelancerRequest request) {
        if (request == null)
            return Problem(title: "Validation error", detail: "Request body is required.", statusCode: StatusCodes.Status400BadRequest);
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
            return Problem(title: "Validation error", detail: "Username and Email are required.", statusCode: StatusCodes.Status400BadRequest);
        if (!request.Email.Contains('@'))
            return Problem(title: "Validation error", detail: "Email must contain '@'.", statusCode: StatusCodes.Status400BadRequest);
        return null;
    }

    /// <summary>
    /// Maps a <see cref="FreelancerRequest"/> to a <see cref="Freelancer"/> domain model.
    /// </summary>
    /// <param name="request">The freelancer request containing input data.</param>
    /// <param name="id">
    /// Optional GUID for the freelancer. If not provided, defaults to <c>Guid.Empty</c>.
    /// Used for setting the <c>FreelancerId</c> in related <see cref="Skillset"/> and <see cref="Hobby"/> objects.
    /// </param>
    /// <returns>
    /// A <see cref="Freelancer"/> instance populated with data from the request.
    /// </returns>
    private static Freelancer MapToModel(FreelancerRequest request, Guid? id = null)
    {
        var model = new Freelancer
        {
            Id = id ?? Guid.Empty,
            Username = request.Username!,
            Email = request.Email!,
            PhoneNumber = request.PhoneNumber ?? string.Empty,
            IsArchived = request.IsArchived
        };

        // Prefer IDs if provided; otherwise map from names (legacy)
        if (request.SkillsetIds != null && request.SkillsetIds.Count > 0)
        {
            model.FreelancerSkillsets = request.SkillsetIds
                .Where(g => g != Guid.Empty)
                .Distinct()
                .Select(g => new Freelancer_Skillset { SkillsetId = g })
                .ToList();
        }
        else
        {
            model.FreelancerSkillsets = (request.Skillsets ?? new List<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => new Freelancer_Skillset { Skillset = new Skillset { Name = s } })
                .ToList();
        }

        if (request.HobbyIds != null && request.HobbyIds.Count > 0)
        {
            model.FreelancerHobbies = request.HobbyIds
                .Where(g => g != Guid.Empty)
                .Distinct()
                .Select(g => new Freelancer_Hobby { HobbyId = g })
                .ToList();
        }
        else
        {
            model.FreelancerHobbies = (request.Hobbies ?? new List<string>())
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(h => new Freelancer_Hobby { Hobby = new Hobby { Name = h } })
                .ToList();
        }

        return model;
    }

    // Global exception handling via ProblemDetails now maps DuplicateFreelancerException -> HTTP 409.

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
        var result = await _repo.GetPagedAsync(page, pageSize, includeArchived, skill, hobby, term);
        // Flatten joins to expected arrays for the frontend
        var shaped = new {
            totalCount = result.TotalCount,
            page = result.Page,
            pageSize = result.PageSize,
            totalPages = result.TotalPages,
            items = result.Items.Select(f => new {
                id = f.Id,
                username = f.Username,
                email = f.Email,
                phoneNumber = f.PhoneNumber,
                isArchived = f.IsArchived,
                skillsets = f.FreelancerSkillsets.Select(fs => new { id = fs.Skillset.Id, name = fs.Skillset.Name }),
                hobbies = f.FreelancerHobbies.Select(fh => new { id = fh.Hobby.Id, name = fh.Hobby.Name })
            })
        };
        return Ok(shaped);
    }

    /// <summary>
    /// Get a single freelancer by Id.
    /// </summary>
    /// <param name="id">Freelancer GUID.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Freelancer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOne(Guid id) {
    var f = await _repo.GetAsync(id);
        if (f is null) return NotFound();
        var shaped = new {
            id = f.Id,
            username = f.Username,
            email = f.Email,
            phoneNumber = f.PhoneNumber,
            isArchived = f.IsArchived,
            skillsets = f.FreelancerSkillsets.Select(fs => new { id = fs.Skillset.Id, name = fs.Skillset.Name }),
            hobbies = f.FreelancerHobbies.Select(fh => new { id = fh.Hobby.Id, name = fh.Hobby.Name })
        };
        return Ok(shaped);
    }

    // Removed separate /search endpoint: unified via optional 'term' in GET

    /// <summary>
    /// Create a new freelancer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Freelancer), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] FreelancerRequest request) {
        var validation = ValidateFreelancerRequest(request);
        if (validation != null) return validation;
        // Enforce IDs-only for skills/hobbies
        if ((request.Skillsets != null && request.Skillsets.Count > 0) || (request.Hobbies != null && request.Hobbies.Count > 0))
            return Problem(title: "Invalid payload", detail: "Use skillsetIds and hobbyIds; names are not allowed.", statusCode: StatusCodes.Status400BadRequest);
        // If ids provided, ensure all exist
        if (request.SkillsetIds != null && request.SkillsetIds.Count > 0) {
            var ids = request.SkillsetIds.Where(g => g != Guid.Empty).Distinct().ToList();
            var count = await _ctx.Skillsets.CountAsync(s => ids.Contains(s.Id));
            if (count != ids.Count) return Problem(title: "Invalid skill ids", detail: "One or more skill ids do not exist.", statusCode: StatusCodes.Status400BadRequest);
        }
        if (request.HobbyIds != null && request.HobbyIds.Count > 0) {
            var ids = request.HobbyIds.Where(g => g != Guid.Empty).Distinct().ToList();
            var count = await _ctx.Hobbies.CountAsync(h => ids.Contains(h.Id));
            if (count != ids.Count) return Problem(title: "Invalid hobby ids", detail: "One or more hobby ids do not exist.", statusCode: StatusCodes.Status400BadRequest);
        }
    var model = MapToModel(request); // Id will be new Guid by constructor
    await _repo.AddAsync(model);
        var shaped = new { id = model.Id, username = model.Username, email = model.Email, phoneNumber = model.PhoneNumber, isArchived = model.IsArchived };
        return CreatedAtAction(nameof(GetOne), new { id = model.Id }, shaped);
    }

    /// <summary>
    /// Replace an existing freelancer and its skillsets/hobbies collections.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] FreelancerRequest request) {
        var existing = await _repo.GetAsync(id);
        if (existing == null) return NotFound();
        var validation = ValidateFreelancerRequest(request);
        if (validation != null) return validation;
        if ((request.Skillsets != null && request.Skillsets.Count > 0) || (request.Hobbies != null && request.Hobbies.Count > 0))
            return Problem(title: "Invalid payload", detail: "Use skillsetIds and hobbyIds; names are not allowed.", statusCode: StatusCodes.Status400BadRequest);
        if (request.SkillsetIds != null && request.SkillsetIds.Count > 0) {
            var ids = request.SkillsetIds.Where(g => g != Guid.Empty).Distinct().ToList();
            var count = await _ctx.Skillsets.CountAsync(s => ids.Contains(s.Id));
            if (count != ids.Count) return Problem(title: "Invalid skill ids", detail: "One or more skill ids do not exist.", statusCode: StatusCodes.Status400BadRequest);
        }
        if (request.HobbyIds != null && request.HobbyIds.Count > 0) {
            var ids = request.HobbyIds.Where(g => g != Guid.Empty).Distinct().ToList();
            var count = await _ctx.Hobbies.CountAsync(h => ids.Contains(h.Id));
            if (count != ids.Count) return Problem(title: "Invalid hobby ids", detail: "One or more hobby ids do not exist.", statusCode: StatusCodes.Status400BadRequest);
        }
    var model = MapToModel(request, id);
    await _repo.UpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// Partial update (currently only supports toggling archive state).
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(Guid id, [FromBody] FreelancerPatchRequest request) {
        var existing = await _repo.GetAsync(id);
        if (existing == null) return NotFound();
        if (request?.IsArchived is null) {
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
    public async Task<IActionResult> Delete(Guid id) {
    var existing = await _repo.GetAsync(id);
    if (existing == null) return NotFound();
    await _repo.DeleteAsync(id);
    return NoContent();
    }
}
