using MediatR;

namespace Spur.MediatR;

/// <summary>
/// Abstract base class for MediatR request handlers that return Result types.
/// Provides a convenient base for CQRS command and query handlers.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response value type.</typeparam>
public abstract class ResultHandler<TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse>>
    where TRequest : IRequest<Result<TResponse>>
{
    /// <summary>
    /// Handles the request and returns a Result.
    /// Override this method to implement your handler logic.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public abstract Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Abstract base class for MediatR request handlers that return Result{Unit}.
/// Useful for commands that don't return a value.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public abstract class ResultHandler<TRequest> : IRequestHandler<TRequest, Result<Unit>>
    where TRequest : IRequest<Result<Unit>>
{
    /// <summary>
    /// Handles the request and returns a Result of Unit.
    /// Override this method to implement your handler logic.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result of Unit indicating success or failure.</returns>
    public abstract Task<Result<Unit>> Handle(TRequest request, CancellationToken cancellationToken);
}
