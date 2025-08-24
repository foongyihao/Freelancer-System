namespace CDN.Freelancers.Domain;

/// <summary>
/// Master hobby entity (e.g., "Chess", "Photography"). Unique by Name.
/// </summary>
public class Hobby {
    /// <summary>
    /// Surrogate GUID primary key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Hobby name / label (unique).
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
