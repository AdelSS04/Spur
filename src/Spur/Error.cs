using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Spur;

/// <summary>
/// An immutable, self-describing error with HTTP semantics.
/// </summary>
/// <remarks>
/// <para><see cref="Error"/> is a <c>readonly record struct</c> — zero heap allocation,
/// structural value equality, and immutability are guaranteed.</para>
/// <para>Always create errors via the static factory methods. Never use the default constructor.</para>
/// <para>Error codes MUST use SCREAMING_SNAKE_CASE convention: e.g. USER_NOT_FOUND, ORDER_CONFLICT.</para>
/// </remarks>
[DebuggerDisplay("{Code} ({HttpStatus}): {Message}")]
public readonly record struct Error
{
    // ──────────────────────────────────────────────────────────────────────────────
    // Properties
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Machine-readable identifier. Use SCREAMING_SNAKE_CASE.</summary>
    /// <example>USER_NOT_FOUND</example>
    public string Code { get; init; }

    /// <summary>Human-readable description suitable for logs and API consumers.</summary>
    public string Message { get; init; }

    /// <summary>HTTP status code this error maps to when returned over HTTP.</summary>
    public int HttpStatus { get; init; }

    /// <summary>Semantic category for programmatic error handling decisions.</summary>
    public ErrorCategory Category { get; init; }

    /// <summary>
    /// Optional key-value metadata attached to this error.
    /// Included in Problem Details <c>extensions</c> when present.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Extensions { get; init; }

    /// <summary>
    /// Optional inner error that caused or contributed to this error.
    /// Enables error chaining for complex failure scenarios.
    /// Stored in a wrapper to avoid struct layout cycles.
    /// </summary>
    private readonly ErrorWrapper? _innerWrapper;

    /// <summary>
    /// Gets the inner error if present, otherwise null.
    /// </summary>
    public Error? Inner => _innerWrapper?.Value;

    // ──────────────────────────────────────────────────────────────────────────────
    // Private Constructor
    // ──────────────────────────────────────────────────────────────────────────────

    private Error(
        string code,
        string message,
        int httpStatus,
        ErrorCategory category,
        IReadOnlyDictionary<string, object?>? extensions = null,
        Error? inner = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Code = code;
        Message = message;
        HttpStatus = httpStatus;
        Category = category;
        Extensions = extensions ?? EmptyExtensions;
        _innerWrapper = inner.HasValue ? new ErrorWrapper(inner.Value) : null;
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Static Factory Methods
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Creates a 422 Unprocessable Entity validation error.</summary>
    public static Error Validation(string message, string code = "VALIDATION_ERROR")
        => new(code, message, 422, ErrorCategory.Validation);

    /// <summary>Creates a 422 error with extension metadata from an anonymous object.</summary>
    /// <param name="code">Error code in SCREAMING_SNAKE_CASE.</param>
    /// <param name="message">Human-readable description.</param>
    /// <param name="extensions">Anonymous object whose properties become extension key-value pairs.</param>
    public static Error Validation(string code, string message, object extensions)
        => new(code, message, 422, ErrorCategory.Validation, AnonymousToDictionary(extensions));

    /// <summary>Creates a 404 Not Found error.</summary>
    public static Error NotFound(string message, string code = "NOT_FOUND")
        => new(code, message, 404, ErrorCategory.NotFound);

    /// <summary>Creates a 404 Not Found error with extension metadata.</summary>
    public static Error NotFound(string code, string message, object extensions)
        => new(code, message, 404, ErrorCategory.NotFound, AnonymousToDictionary(extensions));

    /// <summary>Creates a 401 Unauthorized error.</summary>
    public static Error Unauthorized(string message, string code = "UNAUTHORIZED")
        => new(code, message, 401, ErrorCategory.Unauthorized);

    /// <summary>Creates a 401 error with extension metadata.</summary>
    public static Error Unauthorized(string code, string message, object extensions)
        => new(code, message, 401, ErrorCategory.Unauthorized, AnonymousToDictionary(extensions));

    /// <summary>Creates a 403 Forbidden error.</summary>
    public static Error Forbidden(string message, string code = "FORBIDDEN")
        => new(code, message, 403, ErrorCategory.Forbidden);

    /// <summary>Creates a 403 error with extension metadata.</summary>
    public static Error Forbidden(string code, string message, object extensions)
        => new(code, message, 403, ErrorCategory.Forbidden, AnonymousToDictionary(extensions));

    /// <summary>Creates a 409 Conflict error.</summary>
    public static Error Conflict(string message, string code = "CONFLICT")
        => new(code, message, 409, ErrorCategory.Conflict);

    /// <summary>Creates a 409 error with extension metadata.</summary>
    public static Error Conflict(string code, string message, object extensions)
        => new(code, message, 409, ErrorCategory.Conflict, AnonymousToDictionary(extensions));

    /// <summary>Creates a 429 Too Many Requests error.</summary>
    public static Error TooManyRequests(string message, string code = "TOO_MANY_REQUESTS")
        => new(code, message, 429, ErrorCategory.TooManyRequests);

    /// <summary>Creates a 429 error with extension metadata.</summary>
    public static Error TooManyRequests(string code, string message, object extensions)
        => new(code, message, 429, ErrorCategory.TooManyRequests, AnonymousToDictionary(extensions));

    /// <summary>Creates a 500 Internal Server Error for unexpected/unhandled failures.</summary>
    public static Error Unexpected(string message, string code = "UNEXPECTED_ERROR")
        => new(code, message, 500, ErrorCategory.Unexpected);

    /// <summary>Creates a 500 error by wrapping a .NET exception.</summary>
    public static Error Unexpected(Exception exception, string code = "UNEXPECTED_ERROR")
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new(
            code,
            exception.Message,
            500,
            ErrorCategory.Unexpected,
            new Dictionary<string, object?> { ["exceptionType"] = exception.GetType().Name });
    }

    /// <summary>Creates an error with a fully custom HTTP status code and category.</summary>
    public static Error Custom(
        int httpStatus,
        string code,
        string message,
        ErrorCategory category = ErrorCategory.Custom)
        => new(code, message, httpStatus, category);

    /// <summary>Creates a custom error with extension metadata.</summary>
    public static Error Custom(
        int httpStatus,
        string code,
        string message,
        object extensions,
        ErrorCategory category = ErrorCategory.Custom)
        => new(code, message, httpStatus, category, AnonymousToDictionary(extensions));

    // ──────────────────────────────────────────────────────────────────────────────
    // Instance Modification Methods (return new Error — immutable)
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Returns a new Error with the given inner error attached.</summary>
    public Error WithInner(Error inner) => new(Code, Message, HttpStatus, Category, Extensions, inner);

    /// <summary>Returns a new Error with an overridden human-readable message.</summary>
    public Error WithMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return this with { Message = message };
    }

    /// <summary>Returns a new Error with an overridden error code.</summary>
    public Error WithCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return this with { Code = code };
    }

    /// <summary>
    /// Returns a new Error with the given extension metadata merged in.
    /// Existing keys are overwritten if the same key is present in <paramref name="extensions"/>.
    /// </summary>
    /// <param name="extensions">Anonymous object or IDictionary whose properties are merged.</param>
    public Error WithExtensions(object extensions)
    {
        ArgumentNullException.ThrowIfNull(extensions);
        var additional = AnonymousToDictionary(extensions);
        var merged = new Dictionary<string, object?>(Extensions);
        foreach (var (k, v) in additional)
            merged[k] = v;
        return this with { Extensions = merged };
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Predefined Common Errors
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Generic 404. Prefer specific codes for production APIs.</summary>
    public static readonly Error DefaultNotFound =
        NotFound("The requested resource was not found.", "NOT_FOUND");

    /// <summary>Generic 422 validation error.</summary>
    public static readonly Error DefaultValidation =
        Validation("One or more validation errors occurred.", "VALIDATION_ERROR");

    /// <summary>Generic 401 unauthorized error.</summary>
    public static readonly Error DefaultUnauthorized =
        Unauthorized("Authentication is required to access this resource.", "UNAUTHORIZED");

    /// <summary>Generic 403 forbidden error.</summary>
    public static readonly Error DefaultForbidden =
        Forbidden("You do not have permission to perform this action.", "FORBIDDEN");

    /// <summary>Generic 409 conflict error.</summary>
    public static readonly Error DefaultConflict =
        Conflict("The operation conflicts with the current state of the resource.", "CONFLICT");

    /// <summary>Generic 500 unexpected error.</summary>
    public static readonly Error DefaultUnexpected =
        Unexpected("An unexpected error occurred.", "UNEXPECTED_ERROR");

    // ──────────────────────────────────────────────────────────────────────────────
    // Private Helpers
    // ──────────────────────────────────────────────────────────────────────────────

    private static readonly IReadOnlyDictionary<string, object?> EmptyExtensions =
        new Dictionary<string, object?>(0);

    [UnconditionalSuppressMessage("Trimming", "IL2070:UnrecognizedReflectionPattern",
        Justification = "Anonymous object reflection is intentional and trimming-safe for public properties")]
    [UnconditionalSuppressMessage("Trimming", "IL2075:UnrecognizedReflectionPattern",
        Justification = "Anonymous object reflection is intentional and trimming-safe for public properties")]
    private static IReadOnlyDictionary<string, object?> AnonymousToDictionary(object obj)
    {
        if (obj is IReadOnlyDictionary<string, object?> readOnly)
            return readOnly;

        if (obj is IDictionary<string, object?> mutable)
            return new Dictionary<string, object?>(mutable);

        var dict = new Dictionary<string, object?>();
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            dict[prop.Name] = prop.GetValue(obj);
        return dict;
    }

    /// <summary>
    /// Internal wrapper class to avoid struct layout cycles when storing inner Error.
    /// </summary>
    private sealed class ErrorWrapper(Error value)
    {
        public Error Value { get; } = value;
    }
}
