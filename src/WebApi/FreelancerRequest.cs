namespace CDN.Freelancers.WebApi;

/// <summary>
/// Request DTO used for create and update operations on <c>Freelancer</c> resources.
/// </summary>
/// <param name="Username">Public unique username / handle.</param>
/// <param name="Email">Contact email address.</param>
/// <param name="PhoneNumber">Primary phone number (optional).</param>
/// <param name="Skillsets">List of skill names (each becomes a <c>Skillset</c> child entity).</param>
/// <param name="Hobbies">List of hobby names (each becomes a <c>Hobby</c> child entity).</param>
public record FreelancerRequest(string Username, string Email, string PhoneNumber, List<string> Skillsets, List<string> Hobbies);
