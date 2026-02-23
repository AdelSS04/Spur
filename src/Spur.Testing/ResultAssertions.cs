namespace Spur.Testing;

/// <summary>
/// Provides fluent assertion methods for Result types.
/// Framework-agnostic and works with xUnit, NUnit, MSTest, and other testing frameworks.
/// </summary>
public static class ResultAssertions
{
    // ──────────────────────────────────────────────────────────────────────────────
    // Success Assertions (Sync)
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Asserts that the result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to assert.</param>
    /// <returns>A context for chaining additional assertions on the success value.</returns>
    /// <exception cref="ResultAssertionException">Thrown when the result is a failure.</exception>
    public static SuccessResultContext<T> ShouldBeSuccess<T>(this Result<T> result)
    {
        if (result.IsFailure)
        {
            throw new ResultAssertionException(
                $"Expected result to be successful, but it failed with error [{result.Error.Code}]: {result.Error.Message}");
        }
        return new SuccessResultContext<T>(result.Value);
    }

    /// <summary>
    /// Asserts that the result is successful and executes an assertion on the value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to assert.</param>
    /// <param name="valueAssertion">The assertion to perform on the success value.</param>
    /// <returns>A context for chaining additional assertions.</returns>
    public static SuccessResultContext<T> ShouldBeSuccess<T>(
        this Result<T> result,
        Action<T> valueAssertion)
    {
        ArgumentNullException.ThrowIfNull(valueAssertion);
        var context = result.ShouldBeSuccess();
        return context.WithValue(valueAssertion);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Failure Assertions (Sync)
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Asserts that the result is a failure.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to assert.</param>
    /// <returns>A context for chaining additional assertions on the error.</returns>
    /// <exception cref="ResultAssertionException">Thrown when the result is successful.</exception>
    public static FailureResultContext<T> ShouldBeFailure<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            throw new ResultAssertionException(
                $"Expected result to be a failure, but it succeeded with value: {result.Value}");
        }
        return new FailureResultContext<T>(result.Error);
    }

    /// <summary>
    /// Asserts that the result is a failure with the specified error code.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to assert.</param>
    /// <param name="expectedCode">The expected error code.</param>
    /// <returns>A context for chaining additional assertions.</returns>
    public static FailureResultContext<T> ShouldBeFailureWithCode<T>(
        this Result<T> result,
        string expectedCode)
    {
        var context = result.ShouldBeFailure();
        return context.WithCode(expectedCode);
    }

    /// <summary>
    /// Asserts that the result is a failure with the specified HTTP status code.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to assert.</param>
    /// <param name="expectedStatus">The expected HTTP status code.</param>
    /// <returns>A context for chaining additional assertions.</returns>
    public static FailureResultContext<T> ShouldBeFailureWithStatus<T>(
        this Result<T> result,
        int expectedStatus)
    {
        var context = result.ShouldBeFailure();
        return context.WithHttpStatus(expectedStatus);
    }

    /// <summary>
    /// Asserts that the result is a failure with the specified error category.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to assert.</param>
    /// <param name="expectedCategory">The expected error category.</param>
    /// <returns>A context for chaining additional assertions.</returns>
    public static FailureResultContext<T> ShouldBeFailureWithCategory<T>(
        this Result<T> result,
        ErrorCategory expectedCategory)
    {
        var context = result.ShouldBeFailure();
        return context.WithCategory(expectedCategory);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Success Assertions (Async)
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Asserts that the result is successful (async version).
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">The task returning the result to assert.</param>
    /// <returns>A task that returns a context for chaining additional assertions.</returns>
    public static async Task<SuccessResultContext<T>> ShouldBeSuccessAsync<T>(
        this Task<Result<T>> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        var result = await resultTask.ConfigureAwait(false);
        return result.ShouldBeSuccess();
    }

    /// <summary>
    /// Asserts that the result is successful and executes an assertion on the value (async version).
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">The task returning the result to assert.</param>
    /// <param name="valueAssertion">The assertion to perform on the success value.</param>
    /// <returns>A task that returns a context for chaining additional assertions.</returns>
    public static async Task<SuccessResultContext<T>> ShouldBeSuccessAsync<T>(
        this Task<Result<T>> resultTask,
        Action<T> valueAssertion)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(valueAssertion);
        var result = await resultTask.ConfigureAwait(false);
        return result.ShouldBeSuccess(valueAssertion);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Failure Assertions (Async)
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Asserts that the result is a failure (async version).
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">The task returning the result to assert.</param>
    /// <returns>A task that returns a context for chaining additional assertions.</returns>
    public static async Task<FailureResultContext<T>> ShouldBeFailureAsync<T>(
        this Task<Result<T>> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        var result = await resultTask.ConfigureAwait(false);
        return result.ShouldBeFailure();
    }

    /// <summary>
    /// Asserts that the result is a failure with the specified error code (async version).
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">The task returning the result to assert.</param>
    /// <param name="expectedCode">The expected error code.</param>
    /// <returns>A task that returns a context for chaining additional assertions.</returns>
    public static async Task<FailureResultContext<T>> ShouldBeFailureWithCodeAsync<T>(
        this Task<Result<T>> resultTask,
        string expectedCode)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        var result = await resultTask.ConfigureAwait(false);
        return result.ShouldBeFailureWithCode(expectedCode);
    }

    /// <summary>
    /// Asserts that the result is a failure with the specified HTTP status code (async version).
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">The task returning the result to assert.</param>
    /// <param name="expectedStatus">The expected HTTP status code.</param>
    /// <returns>A task that returns a context for chaining additional assertions.</returns>
    public static async Task<FailureResultContext<T>> ShouldBeFailureWithStatusAsync<T>(
        this Task<Result<T>> resultTask,
        int expectedStatus)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        var result = await resultTask.ConfigureAwait(false);
        return result.ShouldBeFailureWithStatus(expectedStatus);
    }
}
