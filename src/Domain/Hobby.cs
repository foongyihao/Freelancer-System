namespace CDN.Freelancers.Domain;

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
