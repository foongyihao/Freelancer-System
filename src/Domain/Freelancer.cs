namespace CDN.Freelancers.Domain;

/// <summary>
/// Represents a registered freelancer profile including related skillsets and hobbies.
/// </summary>
public class Freelancer
{
    /// <summary>
    /// Unique identifier (GUID) for the freelancer.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Public username / handle (must be unique).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Contact email address (must be unique).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Primary phone number (optional formatting not enforced here).
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Indicates whether the freelancer has been archived (excluded by default from list queries).
    /// </summary>
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// Collection of skillset entries (one-to-many) describing technical capabilities.
    /// </summary>
    public List<Skillset> Skillsets { get; set; } = new();

    /// <summary>
    /// Collection of hobbies (one-to-many) for additional personal interests.
    /// </summary>
    public List<Hobby> Hobbies { get; set; } = new();
}
