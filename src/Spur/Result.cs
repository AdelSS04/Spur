namespace Spur;

/// <summary>
/// Static entry point for creating <see cref="Result{T}"/> values and starting pipelines.
/// </summary>
public static class Result
{
    // ──────────────────────────────────────────────────────────────────────────────
    // Success Factories
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Creates a successful result containing <paramref name="value"/>.</summary>
    public static Result<T> Success<T>(T value) => new(value);

    /// <summary>Creates a successful result with no meaningful value (void operations).</summary>
    public static Result<Unit> Success() => new(Unit.Value);

    // ──────────────────────────────────────────────────────────────────────────────
    // Failure Factories
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Creates a failed result containing <paramref name="error"/>.</summary>
    public static Result<T> Failure<T>(Error error) => new(error);

    /// <summary>Creates a failed result from error components.</summary>
    public static Result<T> Failure<T>(string code, string message, int httpStatus = 500)
        => new(Error.Custom(httpStatus, code, message));

    // ──────────────────────────────────────────────────────────────────────────────
    // Pipeline Entry Points
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts a pipeline with an initial value.
    /// </summary>
    /// <example>
    /// <code>
    /// return await Result.Start(command)
    ///     .ValidateAsync(validator, ct)
    ///     .ThenAsync(cmd => _repo.CreateAsync(cmd, ct))
    ///     .Map(entity => _mapper.Map&lt;Dto&gt;(entity))
    ///     .ToHttpResult(201);
    /// </code>
    /// </example>
    public static Result<T> Start<T>(T value) => Success(value);

    /// <summary>
    /// Starts a pipeline by invoking an async factory function.
    /// Wraps unhandled exceptions as Error.Unexpected.
    /// </summary>
    public static async Task<Result<T>> StartAsync<T>(Func<Task<T>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        try
        {
            return Success(await factory().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            return Failure<T>(Error.Unexpected(ex));
        }
    }

    /// <summary>
    /// Starts a pipeline by invoking a factory that returns a Result.
    /// Wraps unhandled exceptions as Error.Unexpected.
    /// </summary>
    public static async Task<Result<T>> StartAsync<T>(Func<Task<Result<T>>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        try
        {
            return await factory().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Failure<T>(Error.Unexpected(ex));
        }
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Try / Catch Wrappers
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Executes <paramref name="func"/> and returns its value as a success.
    /// If an exception is thrown, returns an Error.Unexpected failure.
    /// </summary>
    public static Result<T> Try<T>(Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        try { return Success(func()); }
        catch (Exception ex) { return Failure<T>(Error.Unexpected(ex)); }
    }

    /// <summary>Async version of <see cref="Try{T}"/>.</summary>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        try { return Success(await func().ConfigureAwait(false)); }
        catch (Exception ex) { return Failure<T>(Error.Unexpected(ex)); }
    }

    /// <summary>
    /// Async version of Try that accepts a factory returning Result.
    /// </summary>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<Result<T>>> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        try { return await func().ConfigureAwait(false); }
        catch (Exception ex) { return Failure<T>(Error.Unexpected(ex)); }
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Combination Operators
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Combines multiple results into a single result containing a list of all values.
    /// Returns success only if ALL results succeed.
    /// Returns the FIRST failure encountered (fail-fast).
    /// </summary>
    public static Result<IReadOnlyList<T>> Combine<T>(params Result<T>[] results)
    {
        ArgumentNullException.ThrowIfNull(results);
        var values = new List<T>(results.Length);
        foreach (var result in results)
        {
            if (result.IsFailure) return Failure<IReadOnlyList<T>>(result.Error);
            values.Add(result.Value);
        }
        return Success<IReadOnlyList<T>>(values.AsReadOnly());
    }

    /// <summary>
    /// Combines multiple results, collecting ALL failures instead of stopping at the first.
    /// Returns success only if ALL results succeed.
    /// On failure, returns a single aggregated error with all failures in its Extensions.
    /// </summary>
    public static Result<IReadOnlyList<T>> CombineAll<T>(params Result<T>[] results)
    {
        ArgumentNullException.ThrowIfNull(results);
        var values = new List<T>(results.Length);
        var errors = new List<Error>();

        foreach (var result in results)
        {
            if (result.IsSuccess) values.Add(result.Value);
            else errors.Add(result.Error);
        }

        if (errors.Count > 0)
        {
            var errorList = errors.Select(e => new { e.Code, e.Message, e.HttpStatus }).ToArray();
            var combined = Error.Validation(
                "MULTIPLE_ERRORS",
                $"{errors.Count} error(s) occurred.",
                new { errors = errorList });
            return Failure<IReadOnlyList<T>>(combined);
        }

        return Success<IReadOnlyList<T>>(values.AsReadOnly());
    }

    /// <summary>
    /// Combines two results of different types into a tuple if both succeed.
    /// Returns the first failure if either fails.
    /// </summary>
    public static Result<(T1, T2)> Combine<T1, T2>(Result<T1> r1, Result<T2> r2)
    {
        if (r1.IsFailure) return Failure<(T1, T2)>(r1.Error);
        if (r2.IsFailure) return Failure<(T1, T2)>(r2.Error);
        return Success((r1.Value, r2.Value));
    }

    /// <summary>
    /// Combines three results into a tuple if all three succeed.
    /// </summary>
    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(Result<T1> r1, Result<T2> r2, Result<T3> r3)
    {
        if (r1.IsFailure) return Failure<(T1, T2, T3)>(r1.Error);
        if (r2.IsFailure) return Failure<(T1, T2, T3)>(r2.Error);
        if (r3.IsFailure) return Failure<(T1, T2, T3)>(r3.Error);
        return Success((r1.Value, r2.Value, r3.Value));
    }
}
