namespace CDN.Freelancers.Domain;

/// <summary>
/// Master skill entity (e.g., "C#", "SQL", "React").
/// </summary>
public class Skillset
{
    /// <summary>
    /// Surrogate integer primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Skill name / label (unique).
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
