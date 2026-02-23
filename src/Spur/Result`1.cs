using System.Diagnostics;
using Spur.Internal;

namespace Spur;

/// <summary>
/// Represents the outcome of an operation that either succeeds with a value of type
/// <typeparamref name="T"/> or fails with an <see cref="Error"/>.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
/// <remarks>
/// <para><see cref="Result{T}"/> is a <c>readonly struct</c>. On the success path, it produces
/// zero heap allocations. On the failure path, the <see cref="Error"/> struct is stored inline.</para>
/// <para>
/// The primary ways to consume a Result:
/// <list type="bullet">
///   <item><term>Pipeline</term><description>Chain <c>.Then()</c>, <c>.Map()</c>, <c>.Validate()</c> etc.</description></item>
///   <item><term>Match</term><description><c>result.Match(onSuccess: ..., onFailure: ...)</c></description></item>
///   <item><term>Check and access</term><description><c>if (result.IsSuccess) use(result.Value)</c></description></item>
///   <item><term>HTTP terminal</term><description><c>await pipeline.ToHttpResult()</c></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay("{IsSuccess ? \"Success: \" + _value : \"Failure: \" + _error.Code}")]
public readonly struct Result<T>
{
    // ──────────────────────────────────────────────────────────────────────────────
    // Internal State
    // ──────────────────────────────────────────────────────────────────────────────

    private readonly T? _value;
    private readonly Error _error;
    private readonly bool _isSuccess;

    // ──────────────────────────────────────────────────────────────────────────────
    // Constructors (internal — callers use Result.Success / Result.Failure / operators)
    // ──────────────────────────────────────────────────────────────────────────────

    internal Result(T value)
    {
        _value = value;
        _error = default;
        _isSuccess = true;
    }

    internal Result(Error error)
    {
        _value = default;
        _error = error;
        _isSuccess = false;
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Core Properties
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>True when the operation succeeded and <see cref="Value"/> is safe to access.</summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>True when the operation failed and <see cref="Error"/> is safe to access.</summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>
    /// The success value. Only safe when <see cref="IsSuccess"/> is true.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed on a failed Result. Check <see cref="IsSuccess"/> first.
    /// </exception>
    public T Value => _isSuccess ? _value! : ThrowHelper.ThrowValueAccessOnFailure<T>();

    /// <summary>
    /// The error descriptor. Only safe when <see cref="IsFailure"/> is true.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed on a successful Result. Check <see cref="IsFailure"/> first.
    /// </exception>
    public Error Error => !_isSuccess ? _error : ThrowHelper.ThrowErrorAccessOnSuccess();

    // ──────────────────────────────────────────────────────────────────────────────
    // Safe Accessors
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Returns the success value if successful, otherwise the provided default.</summary>
    public T? GetValueOrDefault(T? defaultValue = default)
        => _isSuccess ? _value : defaultValue;

    /// <summary>Returns the error if failed, otherwise the provided default error.</summary>
    public Error GetErrorOrDefault(Error defaultError = default)
        => !_isSuccess ? _error : defaultError;

    // ──────────────────────────────────────────────────────────────────────────────
    // Pattern Matching (Safe Destructuring)
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Safely branches based on success or failure, returning a value of type
    /// <typeparamref name="TResult"/> in both cases. This is the primary safe exit from a Result.
    /// </summary>
    /// <typeparam name="TResult">The return type of both branches.</typeparam>
    /// <param name="onSuccess">Invoked with the value when successful.</param>
    /// <param name="onFailure">Invoked with the error when failed.</param>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return _isSuccess ? onSuccess(_value!) : onFailure(_error);
    }

    /// <summary>Async overload of <see cref="Match{TResult}"/>.</summary>
    public Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onSuccess,
        Func<Error, Task<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return _isSuccess ? onSuccess(_value!) : onFailure(_error);
    }

    /// <summary>
    /// Executes one of two actions based on success or failure (void version of Match).
    /// </summary>
    public void Switch(Action<T> onSuccess, Action<Error> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        if (_isSuccess) onSuccess(_value!);
        else onFailure(_error);
    }

    /// <summary>Async overload of <see cref="Switch"/>.</summary>
    public Task SwitchAsync(Func<T, Task> onSuccess, Func<Error, Task> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return _isSuccess ? onSuccess(_value!) : onFailure(_error);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Escape Hatches
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the value if successful, or throws <see cref="SpurException"/> if failed.
    /// Use only when you cannot use the pipeline and absolutely must have the raw value.
    /// </summary>
    /// <exception cref="SpurException">
    /// Thrown when the result is a failure. Contains the <see cref="Error"/>.
    /// </exception>
    public T Unwrap() => _isSuccess ? _value! : throw new SpurException(_error);

    /// <summary>Returns the value if successful, otherwise returns <paramref name="fallback"/>.</summary>
    public T UnwrapOr(T fallback) => _isSuccess ? _value! : fallback;

    /// <summary>Returns the value if successful, otherwise invokes <paramref name="fallback"/>.</summary>
    public T UnwrapOrElse(Func<Error, T> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return _isSuccess ? _value! : fallback(_error);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Implicit Conversion Operators
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Implicitly creates a successful <see cref="Result{T}"/> from a value.</summary>
    public static implicit operator Result<T>(T value) => new(value);

    /// <summary>Implicitly creates a failed <see cref="Result{T}"/> from an <see cref="Error"/>.</summary>
    public static implicit operator Result<T>(Error error) => new(error);

    // ──────────────────────────────────────────────────────────────────────────────
    // Object Overrides
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Returns a string representation of this Result.</summary>
    public override string ToString()
        => _isSuccess
            ? $"Result.Success<{typeof(T).Name}>({_value})"
            : $"Result.Failure<{typeof(T).Name}>({_error.Code}: {_error.Message})";

    /// <summary>Determines whether the specified object is equal to the current Result.</summary>
    public override bool Equals(object? obj) => false; // Struct — no meaningful equality

    /// <summary>Returns a hash code for this Result.</summary>
    public override int GetHashCode() => _isSuccess ? _value?.GetHashCode() ?? 0 : _error.GetHashCode();
}
