namespace CDN.Freelancers.Domain;

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
