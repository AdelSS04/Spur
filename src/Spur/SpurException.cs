namespace Spur;

/// <summary>
/// The exception thrown by <see cref="Result{T}.Unwrap()"/> when called on a failed result.
/// </summary>
/// <remarks>
/// This exception exists purely as an escape hatch. In production code, prefer
/// <see cref="Result{T}.Match{TResult}"/>, <see cref="Result{T}.GetValueOrDefault"/>,
/// or <see cref="Result{T}.UnwrapOr"/> instead of catching this exception.
/// </remarks>
public sealed class SpurException : Exception
{
    /// <summary>The error that caused this exception.</summary>
    public Error Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpurException"/> class
    /// with the specified error.
    /// </summary>
    /// <param name="error">The error that caused this exception.</param>
    public SpurException(Error error)
        : base($"[{error.Code}] {error.Message}")
    {
        Error = error;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpurException"/> class
    /// with the specified error and inner exception.
    /// </summary>
    /// <param name="error">The error that caused this exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public SpurException(Error error, Exception innerException)
        : base($"[{error.Code}] {error.Message}", innerException)
    {
        Error = error;
    }
}
