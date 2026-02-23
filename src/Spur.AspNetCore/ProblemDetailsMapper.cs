using Microsoft.AspNetCore.Mvc;
using Spur.AspNetCore.Options;

namespace Spur.AspNetCore;

/// <summary>
/// Interface for mapping errors to RFC 7807 Problem Details.
/// </summary>
public interface IProblemDetailsMapper
{
    /// <summary>
    /// Maps an error to a ProblemDetails instance.
    /// </summary>
    /// <param name="error">The error to map.</param>
    /// <returns>A ProblemDetails instance.</returns>
    ProblemDetails ToProblemDetails(Error error);
}

/// <summary>
/// Default implementation of Problem Details mapper.
/// Maps Spur errors to RFC 7807 Problem Details format.
/// </summary>
public sealed class DefaultProblemDetailsMapper : IProblemDetailsMapper
{
    private readonly SpurOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultProblemDetailsMapper"/> class.
    /// </summary>
    /// <param name="options">The Spur options.</param>
    public DefaultProblemDetailsMapper(SpurOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>
    /// Maps an error to a ProblemDetails instance following RFC 7807.
    /// </summary>
    public ProblemDetails ToProblemDetails(Error error)
    {
        var statusCode = _options.CustomStatusMapper?.Invoke(error) ?? error.HttpStatus;

        var problemDetails = new ProblemDetails
        {
            Type = $"{_options.ProblemDetailsTypeBaseUri.TrimEnd('/')}/{error.Code}",
            Title = GetTitleForStatus(statusCode),
            Status = statusCode,
            Detail = error.Message
        };

        // Add error code if enabled
        if (_options.IncludeErrorCode)
        {
            problemDetails.Extensions["errorCode"] = error.Code;
        }

        // Add error category if enabled
        if (_options.IncludeErrorCategory)
        {
            problemDetails.Extensions["category"] = error.Category.ToString();
        }

        // Add custom extensions if enabled and present
        if (_options.IncludeCustomExtensions && error.Extensions.Count > 0)
        {
            foreach (var (key, value) in error.Extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        // Add inner errors if enabled and present
        if (_options.IncludeInnerErrors && error.Inner.HasValue)
        {
            problemDetails.Extensions["innerErrors"] = BuildInnerErrorsList(error.Inner.Value);
        }

        return problemDetails;
    }

    private static string GetTitleForStatus(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        429 => "Too Many Requests",
        500 => "Internal Server Error",
        _ => "Error"
    };

    private List<object> BuildInnerErrorsList(Error innerError)
    {
        var innerErrors = new List<object>();
        var current = innerError;

        while (true)
        {
            var errorObject = new Dictionary<string, object?>
            {
                ["code"] = current.Code,
                ["message"] = current.Message,
                ["httpStatus"] = current.HttpStatus
            };

            if (_options.IncludeErrorCategory)
            {
                errorObject["category"] = current.Category.ToString();
            }

            if (_options.IncludeCustomExtensions && current.Extensions.Count > 0)
            {
                errorObject["extensions"] = current.Extensions;
            }

            innerErrors.Add(errorObject);

            if (!current.Inner.HasValue)
            {
                break;
            }

            current = current.Inner.Value;
        }

        return innerErrors;
    }
}
