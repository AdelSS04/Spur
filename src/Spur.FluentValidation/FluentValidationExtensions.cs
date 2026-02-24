using FluentValidation;
using FluentValidation.Results;

namespace Spur.FluentValidation;

/// <summary>
/// Extension methods to bridge FluentValidation with Spur patterns.
/// </summary>
public static class FluentValidationExtensions
{
    /// <summary>
    /// Validates an instance and returns a Result.
    /// Success if validation passes, Validation error if validation fails.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="validator">The validator instance.</param>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A Result indicating validation success or failure.</returns>
    public static Result<T> ValidateToResult<T>(
        this IValidator<T> validator,
        T instance)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(instance);

        var validationResult = validator.Validate(instance);

        if (validationResult.IsValid)
        {
            return Result.Success(instance);
        }

        return CreateValidationError<T>(validationResult);
    }

    /// <summary>
    /// Validates an instance asynchronously and returns a Result.
    /// </summary>
    public static async Task<Result<T>> ValidateToResultAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(instance);

        var validationResult = await validator.ValidateAsync(instance, cancellationToken).ConfigureAwait(false);

        if (validationResult.IsValid)
        {
            return Result.Success(instance);
        }

        return CreateValidationError<T>(validationResult);
    }

    /// <summary>
    /// Pipeline extension: validates the current value using the specified validator.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="result">The current result in the pipeline.</param>
    /// <param name="validator">The validator to apply.</param>
    /// <returns>The same result if validation passes, or a validation error.</returns>
    public static Result<T> Validate<T>(
        this Result<T> result,
        IValidator<T> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        if (result.IsFailure)
        {
            return result;
        }

        return validator.ValidateToResult(result.Value);
    }

    /// <summary>
    /// Async pipeline extension: validates the current value using the specified validator.
    /// </summary>
    public static async Task<Result<T>> ValidateAsync<T>(
        this Result<T> result,
        IValidator<T> validator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validator);

        if (result.IsFailure)
        {
            return result;
        }

        return await validator.ValidateToResultAsync(result.Value, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Async pipeline extension for Task&lt;Result&lt;T&gt;&gt;: awaits then validates.
    /// </summary>
    public static async Task<Result<T>> ValidateAsync<T>(
        this Task<Result<T>> resultTask,
        IValidator<T> validator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(validator);

        var result = await resultTask.ConfigureAwait(false);
        return await result.ValidateAsync(validator, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a validation error from FluentValidation results.
    /// Aggregates all validation failures into a single error with structured extensions.
    /// </summary>
    private static Error CreateValidationError<T>(ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => (object)g.Select(e => e.ErrorMessage).ToArray());

        var message = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));

        return Error.Validation(
            message: message.Length > 0 ? message : "Validation failed.",
            code: "VALIDATION_FAILED")
            .WithExtensions(new { errors });
    }
}
