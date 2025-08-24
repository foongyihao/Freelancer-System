namespace CDN.Freelancers.Domain;

/// <summary>
/// Aggregate root representing a freelancer profile, including basic contact info
/// and many-to-many relationships to master Skillset and Hobby entities via join tables.
/// </summary>
public class Freelancer {
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
    /// Many-to-many link rows to associated skills (join entity rows).
    /// </summary>
    public List<Freelancer_Skillset> FreelancerSkillsets { get; set; } = new();

    /// <summary>
    /// Many-to-many link rows to associated hobbies (join entity rows).
    /// </summary>
    public List<Freelancer_Hobby> FreelancerHobbies { get; set; } = new();
}
