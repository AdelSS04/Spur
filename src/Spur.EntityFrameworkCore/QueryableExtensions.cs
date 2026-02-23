using Microsoft.EntityFrameworkCore;

namespace Spur.EntityFrameworkCore;

/// <summary>
/// Extension methods for IQueryable to support Spur patterns.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Returns the first element of a sequence as a Result, or a NotFound error if no element is found.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">The IQueryable to return the first element of.</param>
    /// <param name="notFoundError">Optional custom error to return when no element is found.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the first element or a NotFound error.</returns>
    public static async Task<Result<T>> FirstOrResultAsync<T>(
        this IQueryable<T> source,
        Error? notFoundError = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var item = await source.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (item is null)
        {
            return notFoundError ?? Error.NotFound($"No {typeof(T).Name} found.");
        }

        return Result.Success(item);
    }

    /// <summary>
    /// Returns the first element of a sequence that satisfies a condition as a Result,
    /// or a NotFound error if no such element is found.
    /// </summary>
    public static async Task<Result<T>> FirstOrResultAsync<T>(
        this IQueryable<T> source,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        Error? notFoundError = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var item = await source.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);

        if (item is null)
        {
            return notFoundError ?? Error.NotFound($"No {typeof(T).Name} found matching the criteria.");
        }

        return Result.Success(item);
    }

    /// <summary>
    /// Returns the only element of a sequence as a Result, or a NotFound error if no element exists,
    /// or an Unexpected error if more than one element exists.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">The IQueryable to return the single element of.</param>
    /// <param name="notFoundError">Optional custom error to return when no element is found.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the single element or an appropriate error.</returns>
    public static async Task<Result<T>> SingleOrResultAsync<T>(
        this IQueryable<T> source,
        Error? notFoundError = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        try
        {
            var item = await source.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (item is null)
            {
                return notFoundError ?? Error.NotFound($"No {typeof(T).Name} found.");
            }

            return Result.Success(item);
        }
        catch (InvalidOperationException)
        {
            return Error.Unexpected(
                $"Multiple {typeof(T).Name} instances found when only one was expected.",
                "MULTIPLE_RESULTS_ERROR");
        }
    }

    /// <summary>
    /// Returns the only element of a sequence that satisfies a condition as a Result,
    /// or a NotFound error if no such element exists, or an Unexpected error if more than one exists.
    /// </summary>
    public static async Task<Result<T>> SingleOrResultAsync<T>(
        this IQueryable<T> source,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        Error? notFoundError = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        try
        {
            var item = await source.SingleOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);

            if (item is null)
            {
                return notFoundError ?? Error.NotFound($"No {typeof(T).Name} found matching the criteria.");
            }

            return Result.Success(item);
        }
        catch (InvalidOperationException)
        {
            return Error.Unexpected(
                $"Multiple {typeof(T).Name} instances found when only one was expected.",
                "MULTIPLE_RESULTS_ERROR");
        }
    }

    /// <summary>
    /// Checks if any element exists in the sequence and returns a Result.
    /// Returns success if at least one element exists, or NotFound error if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">The IQueryable to check for existence.</param>
    /// <param name="notFoundError">Optional custom error to return when no element is found.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result indicating existence.</returns>
    public static async Task<Result<Unit>> ExistsOrResultAsync<T>(
        this IQueryable<T> source,
        Error? notFoundError = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var exists = await source.AnyAsync(cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            return notFoundError ?? Error.NotFound($"No {typeof(T).Name} found.");
        }

        return Result.Success(Unit.Value);
    }

    /// <summary>
    /// Checks if any element satisfying the predicate exists and returns a Result.
    /// </summary>
    public static async Task<Result<Unit>> ExistsOrResultAsync<T>(
        this IQueryable<T> source,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        Error? notFoundError = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var exists = await source.AnyAsync(predicate, cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            return notFoundError ?? Error.NotFound($"No {typeof(T).Name} found matching the criteria.");
        }

        return Result.Success(Unit.Value);
    }

    /// <summary>
    /// Executes the query and returns all elements as a Result containing a list.
    /// Always succeeds (empty list is a valid success).
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">The IQueryable to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the list of elements.</returns>
    public static async Task<Result<IReadOnlyList<T>>> ToResultListAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success<IReadOnlyList<T>>(list);
    }

    /// <summary>
    /// Executes the query and returns all elements as a Result containing a list.
    /// Returns NotFound error if the list is empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">The IQueryable to execute.</param>
    /// <param name="notFoundError">Optional custom error to return when the list is empty.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the list of elements or a NotFound error.</returns>
    public static async Task<Result<IReadOnlyList<T>>> ToResultListOrFailAsync<T>(
        this IQueryable<T> source,
        Error? notFoundError = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = await source.ToListAsync(cancellationToken).ConfigureAwait(false);

        if (list.Count == 0)
        {
            return notFoundError ?? Error.NotFound($"No {typeof(T).Name} records found.");
        }

        return Result.Success<IReadOnlyList<T>>(list);
    }
}
