using Microsoft.AspNetCore.Mvc;

namespace Spur.AspNetCore;

/// <summary>
/// Extension methods for converting Result types to IActionResult for MVC controllers.
/// </summary>
public static class ActionResultExtensions
{
    /// <summary>
    /// Converts a Result to an IActionResult for MVC controller actions.
    /// Success returns 200 OK with the value.
    /// Failure returns ObjectResult with Problem Details and appropriate status code.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="mapper">The Problem Details mapper.</param>
    /// <returns>An IActionResult instance.</returns>
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        IProblemDetailsMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return result.IsSuccess
            ? new OkObjectResult(result.Value)
            : new ObjectResult(mapper.ToProblemDetails(result.Error))
            {
                StatusCode = result.Error.HttpStatus
            };
    }

    /// <summary>
    /// Converts a Result{Unit} to an IActionResult for MVC controller actions.
    /// Success returns 204 No Content.
    /// Failure returns ObjectResult with Problem Details and appropriate status code.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="mapper">The Problem Details mapper.</param>
    /// <returns>An IActionResult instance.</returns>
    public static IActionResult ToActionResult(
        this Result<Unit> result,
        IProblemDetailsMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return result.IsSuccess
            ? new NoContentResult()
            : new ObjectResult(mapper.ToProblemDetails(result.Error))
            {
                StatusCode = result.Error.HttpStatus
            };
    }

    /// <summary>
    /// Converts a Result to an IActionResult with a custom success status code.
    /// Failure returns ObjectResult with Problem Details and appropriate status code.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="mapper">The Problem Details mapper.</param>
    /// <param name="successStatusCode">The HTTP status code for success.</param>
    /// <returns>An IActionResult instance.</returns>
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        IProblemDetailsMapper mapper,
        int successStatusCode)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (result.IsSuccess)
        {
            return successStatusCode switch
            {
                200 => new OkObjectResult(result.Value),
                201 => new CreatedResult(string.Empty, result.Value),
                202 => new AcceptedResult(null as string, result.Value),
                204 => new NoContentResult(),
                _ => new ObjectResult(result.Value) { StatusCode = successStatusCode }
            };
        }

        return new ObjectResult(mapper.ToProblemDetails(result.Error))
        {
            StatusCode = result.Error.HttpStatus
        };
    }

    /// <summary>
    /// Converts a Result to an IActionResult with 201 Created status.
    /// Success returns CreatedResult with the specified location.
    /// Failure returns ObjectResult with Problem Details and appropriate status code.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="mapper">The Problem Details mapper.</param>
    /// <param name="locationUri">The URI of the created resource.</param>
    /// <returns>An IActionResult instance.</returns>
    public static IActionResult ToActionResultCreated<T>(
        this Result<T> result,
        IProblemDetailsMapper mapper,
        string locationUri)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentException.ThrowIfNullOrWhiteSpace(locationUri);

        return result.IsSuccess
            ? new CreatedResult(locationUri, result.Value)
            : new ObjectResult(mapper.ToProblemDetails(result.Error))
            {
                StatusCode = result.Error.HttpStatus
            };
    }

    /// <summary>
    /// Async version: Awaits the result task, then converts to IActionResult.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<T>(
        this Task<Result<T>> resultTask,
        IProblemDetailsMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        return result.ToActionResult(mapper);
    }

    /// <summary>
    /// Async version for Result{Unit}.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync(
        this Task<Result<Unit>> resultTask,
        IProblemDetailsMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        return result.ToActionResult(mapper);
    }

    /// <summary>
    /// Async version with custom success status code.
    /// </summary>
    public static async Task<IActionResult> ToActionResultAsync<T>(
        this Task<Result<T>> resultTask,
        IProblemDetailsMapper mapper,
        int successStatusCode)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        return result.ToActionResult(mapper, successStatusCode);
    }

    /// <summary>
    /// Async version for created resources.
    /// </summary>
    public static async Task<IActionResult> ToActionResultCreatedAsync<T>(
        this Task<Result<T>> resultTask,
        IProblemDetailsMapper mapper,
        string locationUri)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentException.ThrowIfNullOrWhiteSpace(locationUri);

        var result = await resultTask.ConfigureAwait(false);
        return result.ToActionResultCreated(mapper, locationUri);
    }
}
