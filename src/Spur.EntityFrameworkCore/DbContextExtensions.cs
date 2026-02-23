using Microsoft.EntityFrameworkCore;
using Spur.Pipeline;

namespace Spur.EntityFrameworkCore;

/// <summary>
/// Extension methods for DbContext to support Spur patterns.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Saves all changes made in this context to the database as a Result.
    /// Captures DbUpdateException and converts to appropriate error.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the number of state entries written to the database.</returns>
    public static async Task<Result<int>> SaveChangesResultAsync(
        this DbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            var count = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success(count);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return Error.Conflict(
                "A concurrency conflict occurred while saving changes. The record may have been modified or deleted by another user.",
                "DB_CONCURRENCY_ERROR")
                .WithExtensions(new { exception = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            // Check for common constraint violations
            var errorMessage = ex.InnerException?.Message ?? ex.Message;

            if (IsUniqueConstraintViolation(errorMessage))
            {
                return Error.Conflict(
                    "A record with the same unique key already exists.",
                    "DB_UNIQUE_CONSTRAINT_VIOLATION")
                    .WithExtensions(new { exception = errorMessage });
            }

            if (IsForeignKeyViolation(errorMessage))
            {
                return Error.Conflict(
                    "The operation violates a foreign key constraint.",
                    "DB_FOREIGN_KEY_VIOLATION")
                    .WithExtensions(new { exception = errorMessage });
            }

            // Generic database error
            return Error.Unexpected(
                "An error occurred while saving changes to the database.",
                "DB_UPDATE_ERROR")
                .WithExtensions(new { exception = errorMessage });
        }
    }

    /// <summary>
    /// Saves all changes made in this context to the database as a Result of Unit.
    /// Useful when you don't need the count of affected entries.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="acceptAllChangesOnSuccess">Whether to accept all changes on success (unused, for signature compatibility).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result of Unit indicating success or failure.</returns>
    public static async Task<Result<Unit>> SaveChangesResultAsync(
        this DbContext context,
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = await SaveChangesResultAsync(context, cancellationToken).ConfigureAwait(false);
        return result.Map(_ => Unit.Value);
    }

    private static bool IsUniqueConstraintViolation(string errorMessage)
    {
        // Common patterns across different database providers
        return errorMessage.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("unique index", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("UNIQUE KEY", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("cannot insert duplicate", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsForeignKeyViolation(string errorMessage)
    {
        // Common patterns across different database providers
        return errorMessage.Contains("foreign key constraint", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("violates foreign key", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase);
    }
}
