namespace Spur.Pipeline;

/// <summary>
/// Extension methods for transforming success values without the possibility of failure.
/// If the transformation can fail, use ThenExtensions.Then instead.
/// </summary>
public static class MapExtensions
{
    // ──────────────────────────────────────────────────────────────────────────────
    // Sync
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Transforms the success value. On failure, propagates the failure unchanged.</summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        return result.IsSuccess
            ? Result.Success(transform(result.Value))
            : Result.Failure<TOut>(result.Error);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Async: Task<Result<TIn>> → Task<Result<TOut>>
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Awaits the result task, then transforms the success value synchronously.</summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        var result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess
            ? Result.Success(transform(result.Value))
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>Awaits the result task, then transforms the success value asynchronously.</summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        var result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess
            ? Result.Success(await transform(result.Value).ConfigureAwait(false))
            : Result.Failure<TOut>(result.Error);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Async: Result<TIn> (sync) → Task<Result<TOut>>
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Transforms the success value of a synchronous Result asynchronously.</summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        return result.IsSuccess
            ? Result.Success(await transform(result.Value).ConfigureAwait(false))
            : Result.Failure<TOut>(result.Error);
    }
}
