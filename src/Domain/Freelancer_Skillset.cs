namespace CDN.Freelancers.Domain;

/// <summary>
/// Join entity for many-to-many relationship between Freelancer and Skillset.
/// Backed by a table 'freelancer_skillset'.
/// </summary>
public class Freelancer_Skillset
{
    public Guid FreelancerId { get; set; }
    public Freelancer Freelancer { get; set; } = default!;

    public int SkillsetId { get; set; }
    public Skillset Skillset { get; set; } = default!;
}
