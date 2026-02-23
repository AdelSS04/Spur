namespace Spur.Pipeline;

/// <summary>
/// Extension methods for chaining transformations that themselves can fail.
/// Use <c>Then</c> when the next step returns a <see cref="Result{TOut}"/>.
/// Use <see cref="MapExtensions.Map"/> when the transformation cannot fail.
/// </summary>
public static class ThenExtensions
{
    // ──────────────────────────────────────────────────────────────────────────────
    // Sync: Result<TIn> → Result<TOut>
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// If the result is successful, applies <paramref name="next"/> to the value.
    /// If the result is failed, the failure is propagated without calling <paramref name="next"/>.
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        return result.IsSuccess ? next(result.Value) : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Then overload where <paramref name="next"/> may return null.
    /// A null return is treated as a failure with <paramref name="onNull"/>.
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut?> next,
        Error onNull)
        where TOut : class
    {
        ArgumentNullException.ThrowIfNull(next);
        if (result.IsFailure) return Result.Failure<TOut>(result.Error);
        var value = next(result.Value);
        return value is null ? Result.Failure<TOut>(onNull) : Result.Success(value);
    }

    /// <summary>
    /// Then overload for nullable value types.
    /// A null return is treated as a failure with <paramref name="onNull"/>.
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut?> next,
        Error onNull)
        where TOut : struct
    {
        ArgumentNullException.ThrowIfNull(next);
        if (result.IsFailure) return Result.Failure<TOut>(result.Error);
        var value = next(result.Value);
        return value is null ? Result.Failure<TOut>(onNull) : Result.Success(value.Value);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Async: Task<Result<TIn>> → Task<Result<TOut>>
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Async Then: awaits the result task, then chains an async transformation.</summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        var result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess
            ? await next(result.Value).ConfigureAwait(false)
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>Async Then: awaits the result task, then chains a sync transformation.</summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        var result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess ? next(result.Value) : Result.Failure<TOut>(result.Error);
    }

    /// <summary>Async Then with null-check: awaits result, applies async func, handles null as error.</summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut?>> next,
        Error onNull)
        where TOut : class
    {
        ArgumentNullException.ThrowIfNull(next);
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return Result.Failure<TOut>(result.Error);
        var value = await next(result.Value).ConfigureAwait(false);
        return value is null ? Result.Failure<TOut>(onNull) : Result.Success(value);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Async: Result<TIn> (sync) → Task<Result<TOut>>
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Chains an async transformation from a synchronous Result.</summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        return result.IsSuccess
            ? await next(result.Value).ConfigureAwait(false)
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>Async Then with null-check from a synchronous Result.</summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut?>> next,
        Error onNull)
        where TOut : class
    {
        ArgumentNullException.ThrowIfNull(next);
        if (result.IsFailure) return Result.Failure<TOut>(result.Error);
        var value = await next(result.Value).ConfigureAwait(false);
        return value is null ? Result.Failure<TOut>(onNull) : Result.Success(value);
    }
}
