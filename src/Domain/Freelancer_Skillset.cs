namespace CDN.Freelancers.Domain;

/// <summary>
/// Join entity for many-to-many relationship between <see cref="Freelancer"/> and <see cref="Skillset"/>.
/// Backed by table 'freelancer_skillcet' with composite primary key (FreelancerId, SkillsetId).
/// </summary>
public class Freelancer_Skillset {
    /// <summary>
    /// Foreign key to Freelancer.Id
    /// </summary>
    public Guid FreelancerId { get; set; }
    public Freelancer Freelancer { get; set; } = default!;
    /// <summary>
    /// Foreign key to Skillset.Id
    /// </summary>
    public Guid SkillsetId { get; set; }
    public Skillset Skillset { get; set; } = default!;
}
