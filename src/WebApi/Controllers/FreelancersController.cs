using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using CDN.Freelancers.Application;
using CDN.Freelancers.Core;

namespace CDN.Freelancers.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FreelancersController : ControllerBase
{
    private readonly IFreelancerRepository _repo;
    public FreelancersController(IFreelancerRepository repo) => _repo = repo;

    /// <summary>
    /// List freelancers (non-archived by default).
    /// </summary>
    /// <param name="includeArchived">Includes freelancers who are archived when true.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Freelancer>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery, Description("Includes freelancers who are archived when true.")] bool includeArchived = false)
        => Ok(await _repo.GetAllAsync(includeArchived));

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

    /// <summary>
    /// Wildcard search over Username and Email (case-insensitive).
    /// </summary>
    /// <param name="term">Substring to match in username or email.</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<Freelancer>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery, Description("Case-insensitive substring matched against username or email.")] string term)
        => Ok(await _repo.SearchAsync(term));

    /// <summary>
    /// Create a new freelancer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Freelancer), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] FreelancerRequest request)
    {
        var model = new Freelancer
        {
            Username = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Skillsets = request.Skillsets.Select(s => new Skillset { Name = s }).ToList(),
            Hobbies = request.Hobbies.Select(h => new Hobby { Name = h }).ToList()
        };
        await _repo.AddAsync(model);
        return CreatedAtAction(nameof(GetOne), new { id = model.Id }, model);
    }

    /// <summary>
    /// Replace an existing freelancer and its skillsets/hobbies collections.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid id, [FromBody] FreelancerRequest request)
    {
        var model = new Freelancer
        {
            Id = id,
            Username = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Skillsets = request.Skillsets.Select(s => new Skillset { Name = s, FreelancerId = id }).ToList(),
            Hobbies = request.Hobbies.Select(h => new Hobby { Name = h, FreelancerId = id }).ToList()
        };
        await _repo.UpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// Archive or unarchive a freelancer.
    /// </summary>
    /// <param name="id">Freelancer GUID.</param>
    /// <param name="archive">True to archive, false to unarchive.</param>
    [HttpPatch("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Archive(Guid id, [FromQuery, Description("True to archive, false to unarchive.")] bool archive)
    {
        await _repo.ArchiveAsync(id, archive);
        return NoContent();
    }

    /// <summary>
    /// Delete a freelancer and its related collections.
    /// </summary>
    /// <param name="id">Freelancer GUID.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}
