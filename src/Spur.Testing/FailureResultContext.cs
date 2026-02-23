namespace Spur.Testing;

/// <summary>
/// Provides fluent assertion methods for failed results.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public sealed class FailureResultContext<T>
{
    private readonly Error _error;

    /// <summary>
    /// Initializes a new instance of the <see cref="FailureResultContext{T}"/> class.
    /// </summary>
    /// <param name="error">The error from the failed result.</param>
    internal FailureResultContext(Error error)
    {
        _error = error;
    }

    /// <summary>
    /// Gets the error from the failed result.
    /// </summary>
    public Error Error => _error;

    /// <summary>
    /// Asserts that the error code matches the expected code.
    /// </summary>
    /// <param name="expectedCode">The expected error code.</param>
    /// <returns>This context for method chaining.</returns>
    public FailureResultContext<T> WithCode(string expectedCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedCode);
        if (_error.Code != expectedCode)
        {
            throw new ResultAssertionException(
                $"Expected error code '{expectedCode}', but was '{_error.Code}'.");
        }
        return this;
    }

    /// <summary>
    /// Asserts that the HTTP status code matches the expected status.
    /// </summary>
    /// <param name="expectedStatus">The expected HTTP status code.</param>
    /// <returns>This context for method chaining.</returns>
    public FailureResultContext<T> WithHttpStatus(int expectedStatus)
    {
        if (_error.HttpStatus != expectedStatus)
        {
            throw new ResultAssertionException(
                $"Expected HTTP status {expectedStatus}, but was {_error.HttpStatus}.");
        }
        return this;
    }

    /// <summary>
    /// Asserts that the error category matches the expected category.
    /// </summary>
    /// <param name="expectedCategory">The expected error category.</param>
    /// <returns>This context for method chaining.</returns>
    public FailureResultContext<T> WithCategory(ErrorCategory expectedCategory)
    {
        if (_error.Category != expectedCategory)
        {
            throw new ResultAssertionException(
                $"Expected error category '{expectedCategory}', but was '{_error.Category}'.");
        }
        return this;
    }

    /// <summary>
    /// Asserts that the error message matches the expected message exactly.
    /// </summary>
    /// <param name="expectedMessage">The expected error message.</param>
    /// <returns>This context for method chaining.</returns>
    public FailureResultContext<T> WithMessage(string expectedMessage)
    {
        ArgumentNullException.ThrowIfNull(expectedMessage);
        if (_error.Message != expectedMessage)
        {
            throw new ResultAssertionException(
                $"Expected error message '{expectedMessage}', but was '{_error.Message}'.");
        }
        return this;
    }

    /// <summary>
    /// Asserts that the error message contains the expected substring.
    /// </summary>
    /// <param name="expectedSubstring">The expected substring in the error message.</param>
    /// <returns>This context for method chaining.</returns>
    public FailureResultContext<T> WithMessageContaining(string expectedSubstring)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedSubstring);
        if (!_error.Message.Contains(expectedSubstring, StringComparison.Ordinal))
        {
            throw new ResultAssertionException(
                $"Expected error message to contain '{expectedSubstring}', but was '{_error.Message}'.");
        }
        return this;
    }

    /// <summary>
    /// Executes an action against the error for custom assertions.
    /// </summary>
    /// <param name="assertion">The assertion to perform on the error.</param>
    /// <returns>This context for method chaining.</returns>
    public FailureResultContext<T> WithError(Action<Error> assertion)
    {
        ArgumentNullException.ThrowIfNull(assertion);
        try
        {
            assertion(_error);
        }
        catch (Exception ex)
        {
            throw new ResultAssertionException($"Error assertion failed: {ex.Message}", ex);
        }
        return this;
    }

    /// <summary>
    /// Asserts that the error has an inner error with the specified code.
    /// </summary>
    /// <param name="expectedInnerCode">The expected inner error code.</param>
    /// <returns>This context for method chaining.</returns>
    public FailureResultContext<T> WithInnerCode(string expectedInnerCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedInnerCode);

        if (_error.Inner is null)
        {
            throw new ResultAssertionException(
                $"Expected inner error with code '{expectedInnerCode}', but error has no inner error.");
        }

        if (_error.Inner.Value.Code != expectedInnerCode)
        {
            throw new ResultAssertionException(
                $"Expected inner error code '{expectedInnerCode}', but was '{_error.Inner.Value.Code}'.");
        }

        return this;
    }
}
