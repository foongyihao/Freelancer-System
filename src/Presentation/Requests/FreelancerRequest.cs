namespace CDN.Freelancers.Presentation.Requests;

/// <summary>
/// Request DTO used for create and update operations on <c>Freelancer</c> resources.
/// IsArchived is applied during POST (initial state) and PUT (full replacement). For partial archive
/// toggling prefer the PATCH endpoint with a minimal payload: { "isArchived": true }.
/// </summary>
public class FreelancerRequest
{
	/// <summary>Public unique username / handle.</summary>
	public string Username { get; set; } = string.Empty;
	/// <summary>Contact email address.</summary>
	public string Email { get; set; } = string.Empty;
	/// <summary>Primary phone number (optional).</summary>
	public string? PhoneNumber { get; set; }
	/// <summary>Indicates whether the freelancer request is archived.</summary>
	public bool IsArchived { get; set; } = false;
	/// <summary>List of skill names (each becomes a <c>Skillset</c> child entity).</summary>
	public List<string>? Skillsets { get; set; }
	/// <summary>List of hobby names (each becomes a <c>Hobby</c> child entity).</summary>
	public List<string>? Hobbies { get; set; }
}
