namespace CDN.Freelancers.Core;

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
    public string PhoneNumber { get; set; } = string.Empty;

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

/// <summary>
/// A single skill descriptor (e.g., "C#", "SQL", "React").
/// </summary>
public class Skillset
{
    /// <summary>
    /// Surrogate integer primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Skill name / label.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Owning freelancer's GUID foreign key.
    /// </summary>
    public Guid FreelancerId { get; set; }
}

/// <summary>
/// A single hobby descriptor (e.g., "Chess", "Photography").
/// </summary>
public class Hobby
{
    /// <summary>
    /// Surrogate integer primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Hobby name / label.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Owning freelancer's GUID foreign key.
    /// </summary>
    public Guid FreelancerId { get; set; }
}
