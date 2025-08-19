namespace CDN.Freelancers.Presentation.Requests;

/// <summary>
/// Request body for partial update of a freelancer. Currently only supports toggling archive state.
/// </summary>
public class FreelancerPatchRequest
{
    /// <summary>
    /// New archive state for the freelancer. Example: <c>true</c> to archive, <c>false</c> to unarchive.
    /// </summary>
    public bool? IsArchived { get; set; }
}
