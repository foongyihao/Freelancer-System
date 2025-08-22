namespace CDN.Freelancers.Domain;

/// <summary>
/// Master hobby entity (e.g., "Chess", "Photography").
/// </summary>
public class Hobby
{
    /// <summary>
    /// Surrogate integer primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Hobby name / label (unique).
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
