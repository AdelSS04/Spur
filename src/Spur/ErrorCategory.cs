namespace Spur;

/// <summary>
/// Semantic groupings for errors that allow filtering, recovery logic,
/// and HTTP middleware decisions without inspecting status codes directly.
/// </summary>
public enum ErrorCategory
{
    /// <summary>
    /// The submitted input violated validation rules.
    /// Default HTTP status: 422 Unprocessable Entity.
    /// </summary>
    Validation,

    /// <summary>
    /// A requested resource could not be located.
    /// Default HTTP status: 404 Not Found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The caller is not authenticated.
    /// Default HTTP status: 401 Unauthorized.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// The caller is authenticated but lacks permission.
    /// Default HTTP status: 403 Forbidden.
    /// </summary>
    Forbidden,

    /// <summary>
    /// The operation conflicts with current system state.
    /// Default HTTP status: 409 Conflict.
    /// </summary>
    Conflict,

    /// <summary>
    /// The caller has exceeded rate limits.
    /// Default HTTP status: 429 Too Many Requests.
    /// </summary>
    TooManyRequests,

    /// <summary>
    /// An unexpected or unhandled failure occurred.
    /// Default HTTP status: 500 Internal Server Error.
    /// </summary>
    Unexpected,

    /// <summary>
    /// A custom category for domain-specific groupings not covered above.
    /// HTTP status must be set explicitly via Error.Custom().
    /// </summary>
    Custom
}
