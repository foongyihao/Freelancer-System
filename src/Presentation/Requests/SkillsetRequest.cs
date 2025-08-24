using System.ComponentModel;
namespace CDN.Freelancers.Presentation.Requests;

/// <summary>
/// Request DTO used for create and update operations on <c>Skillset</c> resources.
/// Requires a non-empty, unique <see cref="Name"/>. The API enforces case-insensitive uniqueness.
/// </summary>
public class SkillsetRequest
{
    /// <summary>The display name of the skill.</summary>
    [DefaultValue("C#")]
    public string Name { get; set; } = string.Empty;
}
