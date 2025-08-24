namespace CDN.Freelancers.Domain.Exceptions;

/// <summary>
/// Exception thrown when creating or updating a record would violate a uniqueness constraint.
/// </summary>
public class DuplicateRecordException : Exception {
    /// <summary>Gets the name of the duplicate entity.</summary>
    public string EntityName { get; }
    /// <summary>Gets an optional key identifying the duplicate.</summary>
    public object? Key { get; }

    /// <summary>
    /// Creates a new <see cref="DuplicateRecordException"/>.
    /// </summary>
    /// <param name="entityName">The entity name.</param>
    /// <param name="key">Optional key value.</param>
    /// <param name="message">Optional custom message.</param>
    public DuplicateRecordException(string entityName, object? key = null, string? message = null)
        : base(message ?? $"{entityName} with key '{key}' already exists.") {
        EntityName = entityName;
        Key = key;
    }
}
