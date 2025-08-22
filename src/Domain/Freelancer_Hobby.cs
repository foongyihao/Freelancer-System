namespace CDN.Freelancers.Domain;

/// <summary>
/// Join entity for many-to-many relationship between Freelancer and Hobby.
/// Backed by a table 'freelancer_hobby'.
/// </summary>
public class Freelancer_Hobby
{
    public Guid FreelancerId { get; set; }
    public Freelancer Freelancer { get; set; } = default!;

    public int HobbyId { get; set; }
    public Hobby Hobby { get; set; } = default!;
}
