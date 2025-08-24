namespace CDN.Freelancers.Domain;

/// <summary>
/// Join entity for many-to-many relationship between <see cref="Freelancer"/> and <see cref="Hobby"/>.
/// Backed by table 'freelancer_hobby' with composite primary key (FreelancerId, HobbyId).
/// </summary>
public class Freelancer_Hobby {
    /// <summary>
    /// Foreign key to Freelancer.Id
    /// </summary>
    public Guid FreelancerId { get; set; }
    public Freelancer Freelancer { get; set; } = default!;
    /// <summary>
    /// Foreign key to Hobby.Id
    /// </summary>
    public Guid HobbyId { get; set; }
    public Hobby Hobby { get; set; } = default!;
}
