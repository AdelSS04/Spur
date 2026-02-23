using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Spur.AspNetCore;

/// <summary>
/// Optional middleware for handling unhandled exceptions and converting them to Result errors.
/// </summary>
public sealed class SpurMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IProblemDetailsMapper _mapper;
    private readonly ILogger<SpurMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpurMiddleware"/> class.
    /// </summary>
    public SpurMiddleware(
        RequestDelegate next,
        IProblemDetailsMapper mapper,
        ILogger<SpurMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (SpurException ex)
        {
            _logger.LogWarning(ex, "SpurException caught: {ErrorCode}", ex.Error.Code);
            await HandleSpurExceptionAsync(context, ex.Error).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught");
            var error = Error.Unexpected("An unexpected error occurred.", "UNHANDLED_EXCEPTION");
            await HandleSpurExceptionAsync(context, error).ConfigureAwait(false);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "ProblemDetails is a simple DTO with public properties")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "ProblemDetails serialization is AOT-compatible")]
    private async Task HandleSpurExceptionAsync(HttpContext context, Error error)
    {
        var problemDetails = _mapper.ToProblemDetails(error);

        context.Response.StatusCode = error.HttpStatus;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        await context.Response.WriteAsync(json).ConfigureAwait(false);
    }
}
