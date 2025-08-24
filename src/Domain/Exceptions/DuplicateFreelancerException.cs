namespace CDN.Freelancers.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to create or update a freelancer with a duplicate username or email.
/// </summary>
public sealed class DuplicateFreelancerException : Exception
{
    public DuplicateFreelancerException(string message) : base(message) { }
}
