using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Spur.MediatR;

/// <summary>
/// MediatR pipeline behavior that catches unhandled exceptions and wraps them as Result errors.
/// This ensures that handlers returning Result types maintain consistent error handling.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type (must be a Result).</typeparam>
public sealed class ResultPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ResultPipelineBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultPipelineBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ResultPipelineBehavior(ILogger<ResultPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the request and wraps any unhandled exceptions as Result errors.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (SpurException ex)
        {
            _logger.LogWarning(ex, "SpurException caught in pipeline: {ErrorCode}", ex.Error.Code);
            return CreateErrorResponse(ex.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught in MediatR pipeline for request {RequestType}", typeof(TRequest).Name);
            var error = Error.Unexpected(
                "An unexpected error occurred while processing the request.",
                "UNHANDLED_PIPELINE_EXCEPTION")
                .WithExtensions(new { requestType = typeof(TRequest).Name, exceptionType = ex.GetType().Name });
            return CreateErrorResponse(error);
        }
    }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Result.Failure<T> is a public API method that will be preserved")]
    private static TResponse CreateErrorResponse(Error error)
    {
        var responseType = typeof(TResponse);

        // Handle Result<T> where T is a specific type
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result).GetMethod(nameof(Result.Failure))
                ?.MakeGenericMethod(valueType);

            if (failureMethod != null)
            {
                var result = failureMethod.Invoke(null, [error]);
                if (result is TResponse typedResult)
                {
                    return typedResult;
                }
            }
        }

        throw new InvalidOperationException(
            $"ResultPipelineBehavior can only be used with Result<T> response types. Got: {responseType.Name}");
    }
}
