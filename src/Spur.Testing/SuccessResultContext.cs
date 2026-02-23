namespace Spur.Testing;

/// <summary>
/// Provides fluent assertion methods for successful results.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public sealed class SuccessResultContext<T>
{
    private readonly T _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="SuccessResultContext{T}"/> class.
    /// </summary>
    /// <param name="value">The success value.</param>
    internal SuccessResultContext(T value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the success value.
    /// </summary>
    public T Value => _value;

    /// <summary>
    /// Executes an action against the success value for additional assertions.
    /// </summary>
    /// <param name="assertion">The assertion to perform on the value.</param>
    /// <returns>This context for method chaining.</returns>
    public SuccessResultContext<T> WithValue(Action<T> assertion)
    {
        ArgumentNullException.ThrowIfNull(assertion);
        try
        {
            assertion(_value);
        }
        catch (Exception ex)
        {
            throw new ResultAssertionException($"Value assertion failed: {ex.Message}", ex);
        }
        return this;
    }

    /// <summary>
    /// Asserts that the success value equals the expected value.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    /// <returns>This context for method chaining.</returns>
    public SuccessResultContext<T> WithValue(T expected)
    {
        if (!EqualityComparer<T>.Default.Equals(_value, expected))
        {
            throw new ResultAssertionException(
                $"Expected value to be {expected}, but was {_value}.");
        }
        return this;
    }
}
