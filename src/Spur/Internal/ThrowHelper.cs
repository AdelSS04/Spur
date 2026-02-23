using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Spur.Internal;

/// <summary>
/// Internal helpers that enable JIT inlining of property accessors by
/// keeping throw statements out of the hot path.
/// </summary>
internal static class ThrowHelper
{
    /// <summary>
    /// Throws an exception when attempting to access Value on a failed Result.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static T ThrowValueAccessOnFailure<T>()
        => throw new InvalidOperationException(
            "Cannot access Value on a failed Result<T>. " +
            "Check IsSuccess before accessing Value, or use Match(), GetValueOrDefault(), or UnwrapOr().");

    /// <summary>
    /// Throws an exception when attempting to access Error on a successful Result.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static Error ThrowErrorAccessOnSuccess()
        => throw new InvalidOperationException(
            "Cannot access Error on a successful Result<T>. " +
            "Check IsFailure before accessing Error, or use Match() to handle both cases.");
}
