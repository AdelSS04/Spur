namespace Spur.Pipeline;

/// <summary>
/// Extension methods for adding guard checks to a pipeline.
/// Validation steps do not change the success value — they only decide pass or fail.
/// Use these for business rules, authorization checks, and preconditions.
/// </summary>
public static class ValidateExtensions
{
    // ──────────────────────────────────────────────────────────────────────────────
    // Sync
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Checks the predicate against the success value.
    /// If false, returns a failure with <paramref name="errorIfFalse"/>.
    /// If the result is already failed, propagates the failure without calling the predicate.
    /// </summary>
    public static Result<T> Validate<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error errorIfFalse)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (result.IsFailure) return result;
        return predicate(result.Value) ? result : Result.Failure<T>(errorIfFalse);
    }

    /// <summary>
    /// Validates using a predicate. The error is produced by a factory for contextual messages.
    /// </summary>
    public static Result<T> Validate<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Func<T, Error> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);
        if (result.IsFailure) return result;
        return predicate(result.Value) ? result : Result.Failure<T>(errorFactory(result.Value));
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Async: Task<Result<T>> input
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Awaits the result, then applies a sync predicate check.</summary>
    public static async Task<Result<T>> ValidateAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        Error errorIfFalse)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var result = await resultTask.ConfigureAwait(false);
        return result.Validate(predicate, errorIfFalse);
    }

    /// <summary>Awaits the result, then applies an async predicate check.</summary>
    public static async Task<Result<T>> ValidateAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task<bool>> predicate,
        Error errorIfFalse)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return result;
        var passed = await predicate(result.Value).ConfigureAwait(false);
        return passed ? result : Result.Failure<T>(errorIfFalse);
    }

    /// <summary>Awaits the result, then applies an async predicate with error factory.</summary>
    public static async Task<Result<T>> ValidateAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task<bool>> predicate,
        Func<T, Error> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return result;
        var passed = await predicate(result.Value).ConfigureAwait(false);
        return passed ? result : Result.Failure<T>(errorFactory(result.Value));
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Async: Result<T> (sync) input
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Applies an async predicate check to a synchronous Result.</summary>
    public static async Task<Result<T>> ValidateAsync<T>(
        this Result<T> result,
        Func<T, Task<bool>> predicate,
        Error errorIfFalse)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (result.IsFailure) return result;
        var passed = await predicate(result.Value).ConfigureAwait(false);
        return passed ? result : Result.Failure<T>(errorIfFalse);
    }

    /// <summary>Applies an async predicate check with error factory to a synchronous Result.</summary>
    public static async Task<Result<T>> ValidateAsync<T>(
        this Result<T> result,
        Func<T, Task<bool>> predicate,
        Func<T, Error> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);
        if (result.IsFailure) return result;
        var passed = await predicate(result.Value).ConfigureAwait(false);
        return passed ? result : Result.Failure<T>(errorFactory(result.Value));
    }
}
