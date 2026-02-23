namespace Spur.Pipeline;

/// <summary>
/// Extension methods for executing side effects within a pipeline without changing the result.
/// Use for logging, metrics emission, event publishing, caching, etc.
/// The result value is always passed through unchanged.
/// </summary>
public static class TapExtensions
{
    // ──────────────────────────────────────────────────────────────────────────────
    // Sync
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Executes <paramref name="onSuccess"/> if successful. Value passes through unchanged.</summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        if (result.IsSuccess) onSuccess(result.Value);
        return result;
    }

    /// <summary>Executes <paramref name="onError"/> if failed. Result passes through unchanged.</summary>
    public static Result<T> TapError<T>(this Result<T> result, Action<Error> onError)
    {
        ArgumentNullException.ThrowIfNull(onError);
        if (result.IsFailure) onError(result.Error);
        return result;
    }

    /// <summary>Executes the appropriate action regardless of success or failure. Always passes through.</summary>
    public static Result<T> TapBoth<T>(
        this Result<T> result,
        Action<T> onSuccess,
        Action<Error> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        if (result.IsSuccess) onSuccess(result.Value);
        else onError(result.Error);
        return result;
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Async: Task<Result<T>> input
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Awaits the result, executes sync side effect on success, passes through.</summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Action<T> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess) onSuccess(result.Value);
        return result;
    }

    /// <summary>Awaits the result, executes async side effect on success, passes through.</summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess) await onSuccess(result.Value).ConfigureAwait(false);
        return result;
    }

    /// <summary>Awaits the result, executes sync side effect on failure, passes through.</summary>
    public static async Task<Result<T>> TapErrorAsync<T>(
        this Task<Result<T>> resultTask,
        Action<Error> onError)
    {
        ArgumentNullException.ThrowIfNull(onError);
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) onError(result.Error);
        return result;
    }

    /// <summary>Awaits the result, executes async side effect on failure, passes through.</summary>
    public static async Task<Result<T>> TapErrorAsync<T>(
        this Task<Result<T>> resultTask,
        Func<Error, Task> onError)
    {
        ArgumentNullException.ThrowIfNull(onError);
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) await onError(result.Error).ConfigureAwait(false);
        return result;
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Async: Result<T> (sync) input
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Executes async side effect on success from a synchronous Result.</summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Result<T> result,
        Func<T, Task> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        if (result.IsSuccess) await onSuccess(result.Value).ConfigureAwait(false);
        return result;
    }

    /// <summary>Executes async side effect on failure from a synchronous Result.</summary>
    public static async Task<Result<T>> TapErrorAsync<T>(
        this Result<T> result,
        Func<Error, Task> onError)
    {
        ArgumentNullException.ThrowIfNull(onError);
        if (result.IsFailure) await onError(result.Error).ConfigureAwait(false);
        return result;
    }
}
