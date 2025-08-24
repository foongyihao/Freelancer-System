namespace CDN.Freelancers.Domain;

/// <summary>
/// Master skill entity (e.g., "C#", "SQL", "React"). Unique by Name.
/// </summary>
public class Skillset {
    /// <summary>
    /// Surrogate GUID primary key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Skill name / label (unique).
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
