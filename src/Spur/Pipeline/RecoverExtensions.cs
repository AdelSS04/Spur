namespace Spur.Pipeline;

/// <summary>
/// Extension methods for attempting to recover from a pipeline failure.
/// If recovery succeeds, the pipeline continues as a success.
/// If recovery also fails, the recovery failure replaces the original failure.
/// Use these to implement fallback strategies, retries, and circuit breakers.
/// </summary>
public static class RecoverExtensions
{
    // ──────────────────────────────────────────────────────────────────────────────
    // Sync
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// If the result is a failure, invokes <paramref name="recovery"/> with the error.
    /// If successful, passes through unchanged.
    /// </summary>
    public static Result<T> Recover<T>(
        this Result<T> result,
        Func<Error, Result<T>> recovery)
    {
        ArgumentNullException.ThrowIfNull(recovery);
        return result.IsFailure ? recovery(result.Error) : result;
    }

    /// <summary>
    /// If the result is a failure, invokes <paramref name="recovery"/> and wraps as success.
    /// </summary>
    public static Result<T> Recover<T>(
        this Result<T> result,
        Func<Error, T> recovery)
    {
        ArgumentNullException.ThrowIfNull(recovery);
        return result.IsFailure ? Result.Success(recovery(result.Error)) : result;
    }

    /// <summary>
    /// Recovers only when the error's category matches <paramref name="category"/>.
    /// Other error categories are propagated unchanged.
    /// </summary>
    public static Result<T> RecoverIf<T>(
        this Result<T> result,
        ErrorCategory category,
        Func<Error, Result<T>> recovery)
    {
        ArgumentNullException.ThrowIfNull(recovery);
        return result.IsFailure && result.Error.Category == category
            ? recovery(result.Error)
            : result;
    }

    /// <summary>
    /// Recovers only when the error's code matches <paramref name="code"/>.
    /// </summary>
    public static Result<T> RecoverIfCode<T>(
        this Result<T> result,
        string code,
        Func<Error, Result<T>> recovery)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(recovery);
        return result.IsFailure && result.Error.Code == code
            ? recovery(result.Error)
            : result;
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Async: Task<Result<T>> input
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Awaits the result, then applies async recovery if failed.</summary>
    public static async Task<Result<T>> RecoverAsync<T>(
        this Task<Result<T>> resultTask,
        Func<Error, Task<Result<T>>> recovery)
    {
        ArgumentNullException.ThrowIfNull(recovery);
        var result = await resultTask.ConfigureAwait(false);
        return result.IsFailure
            ? await recovery(result.Error).ConfigureAwait(false)
            : result;
    }

    /// <summary>Awaits the result, then applies conditional async recovery by category.</summary>
    public static async Task<Result<T>> RecoverIfAsync<T>(
        this Task<Result<T>> resultTask,
        ErrorCategory category,
        Func<Error, Task<Result<T>>> recovery)
    {
        ArgumentNullException.ThrowIfNull(recovery);
        var result = await resultTask.ConfigureAwait(false);
        return result.IsFailure && result.Error.Category == category
            ? await recovery(result.Error).ConfigureAwait(false)
            : result;
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Async: Result<T> (sync) input
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Applies async recovery to a synchronous Result.</summary>
    public static async Task<Result<T>> RecoverAsync<T>(
        this Result<T> result,
        Func<Error, Task<Result<T>>> recovery)
    {
        ArgumentNullException.ThrowIfNull(recovery);
        return result.IsFailure
            ? await recovery(result.Error).ConfigureAwait(false)
            : result;
    }
}
