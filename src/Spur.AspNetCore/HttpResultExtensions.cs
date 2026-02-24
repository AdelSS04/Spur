using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace Spur.AspNetCore;

/// <summary>
/// Extension methods for converting Result types to IResult for Minimal APIs.
/// </summary>
public static class HttpResultExtensions
{
    /// <summary>
    /// Converts a Result to an IResult for Minimal API endpoints.
    /// Success returns 200 OK with the value as JSON.
    /// Failure returns Problem Details with the appropriate status code.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="mapper">The Problem Details mapper.</param>
    /// <returns>An IResult instance.</returns>
    public static IResult ToHttpResult<T>(
        this Result<T> result,
        IProblemDetailsMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(mapper.ToProblemDetails(result.Error));
    }

    /// <summary>
    /// Converts a Result{Unit} to an IResult for Minimal API endpoints.
    /// Success returns 204 No Content.
    /// Failure returns Problem Details with the appropriate status code.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="mapper">The Problem Details mapper.</param>
    /// <returns>An IResult instance.</returns>
    public static IResult ToHttpResult(
        this Result<Unit> result,
        IProblemDetailsMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(mapper.ToProblemDetails(result.Error));
    }

    /// <summary>
    /// Converts a Result to an IResult with a custom success status code.
    /// Failure returns Problem Details with the appropriate status code.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="mapper">The Problem Details mapper.</param>
    /// <param name="successStatusCode">The HTTP status code for success.</param>
    /// <returns>An IResult instance.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JSON serialization handled by ASP.NET Core runtime")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization handled by ASP.NET Core runtime")]
    public static IResult ToHttpResult<T>(
        this Result<T> result,
        IProblemDetailsMapper mapper,
        int successStatusCode)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (result.IsSuccess)
        {
            return successStatusCode switch
            {
                200 => Results.Ok(result.Value),
                201 => Results.Created(string.Empty, result.Value),
                202 => Results.Accepted(null, result.Value),
                204 => Results.NoContent(),
                _ => Results.Json(result.Value, statusCode: successStatusCode)
            };
        }

        return Results.Problem(mapper.ToProblemDetails(result.Error));
    }

    /// <summary>
    /// Converts a Result to an IResult with a custom success status code for created resources.
    /// Success returns 201 Created with the specified location.
    /// Failure returns Problem Details with the appropriate status code.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="mapper">The Problem Details mapper.</param>
    /// <param name="locationUri">The URI of the created resource.</param>
    /// <returns>An IResult instance.</returns>
    public static IResult ToHttpResultCreated<T>(
        this Result<T> result,
        IProblemDetailsMapper mapper,
        string locationUri)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentException.ThrowIfNullOrWhiteSpace(locationUri);

        return result.IsSuccess
            ? Results.Created(locationUri, result.Value)
            : Results.Problem(mapper.ToProblemDetails(result.Error));
    }

    /// <summary>
    /// Async version: Awaits the result task, then converts to IResult.
    /// </summary>
    public static async Task<IResult> ToHttpResultAsync<T>(
        this Task<Result<T>> resultTask,
        IProblemDetailsMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        return result.ToHttpResult(mapper);
    }

    /// <summary>
    /// Async version for Result{Unit}: Awaits the result task, then converts to IResult.
    /// </summary>
    public static async Task<IResult> ToHttpResultAsync(
        this Task<Result<Unit>> resultTask,
        IProblemDetailsMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        return result.ToHttpResult(mapper);
    }

    /// <summary>
    /// Async version with custom success status code.
    /// </summary>
    public static async Task<IResult> ToHttpResultAsync<T>(
        this Task<Result<T>> resultTask,
        IProblemDetailsMapper mapper,
        int successStatusCode)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        return result.ToHttpResult(mapper, successStatusCode);
    }

    /// <summary>
    /// Async version for created resources.
    /// </summary>
    public static async Task<IResult> ToHttpResultCreatedAsync<T>(
        this Task<Result<T>> resultTask,
        IProblemDetailsMapper mapper,
        string locationUri)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentException.ThrowIfNullOrWhiteSpace(locationUri);

        var result = await resultTask.ConfigureAwait(false);
        return result.ToHttpResultCreated(mapper, locationUri);
    }
}
