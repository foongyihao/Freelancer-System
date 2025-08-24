namespace CDN.Freelancers.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested entity cannot be found.
/// </summary>
public class EntityNotFoundException : Exception {
    /// <summary>Gets the name of the missing entity.</summary>
    public string EntityName { get; }
    /// <summary>Gets the key value used to look up the entity.</summary>
    public object? Key { get; }

    /// <summary>
    /// Creates a new <see cref="EntityNotFoundException"/>.
    /// </summary>
    /// <param name="entityName">The entity name.</param>
    /// <param name="key">The key value.</param>
    /// <param name="message">Optional custom message.</param>
    public EntityNotFoundException(string entityName, object? key = null, string? message = null)
        : base(message ?? $"{entityName} with key '{key}' was not found.") {
        EntityName = entityName;
        Key = key;
    }
}
