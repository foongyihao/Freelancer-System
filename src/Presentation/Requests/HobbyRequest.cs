using System.ComponentModel;
namespace CDN.Freelancers.Presentation.Requests;

/// <summary>
/// Request DTO used for create and update operations on <c>Hobby</c> resources.
/// Requires a non-empty, unique <see cref="Name"/>. The API enforces case-insensitive uniqueness.
/// </summary>
public class HobbyRequest
{
    /// <summary>The display name of the hobby.</summary>
    [DefaultValue("Chess")]
    public string Name { get; set; } = string.Empty;
}
