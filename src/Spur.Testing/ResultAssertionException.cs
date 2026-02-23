namespace Spur.Testing;

/// <summary>
/// Exception thrown when a Result assertion fails.
/// </summary>
public sealed class ResultAssertionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultAssertionException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ResultAssertionException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultAssertionException"/> class
    /// with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ResultAssertionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
