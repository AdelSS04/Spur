namespace Spur.Pipeline;

/// <summary>
/// Extension methods for terminal pattern matching operations.
/// These provide fluent alternatives to the instance Match methods on Result.
/// </summary>
public static class MatchExtensions
{
    /// <summary>
    /// Extension version of <see cref="Result{T}.Match{TResult}"/> for fluent chaining.
    /// Safely branches based on success or failure, returning a value of type
    /// <typeparamref name="TResult"/> in both cases.
    /// </summary>
    public static TResult Match<T, TResult>(
        this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
        => result.Match(onSuccess, onFailure);

    /// <summary>
    /// Async extension version of Match for Task&lt;Result&lt;T&gt;&gt;.
    /// Awaits the result, then applies synchronous matchers.
    /// </summary>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Result<T>> resultTask,
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>
    /// Async extension version of Match for Task&lt;Result&lt;T&gt;&gt;.
    /// Awaits the result, then applies async matchers.
    /// </summary>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Result<T>> resultTask,
        Func<T, Task<TResult>> onSuccess,
        Func<Error, Task<TResult>> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return await result.MatchAsync(onSuccess, onFailure).ConfigureAwait(false);
    }
}
