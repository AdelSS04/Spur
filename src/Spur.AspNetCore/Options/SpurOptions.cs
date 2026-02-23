namespace Spur.AspNetCore.Options;

/// <summary>
/// Configuration options for Spur ASP.NET Core integration.
/// </summary>
public sealed class SpurOptions
{
    /// <summary>
    /// Gets or sets the base URI for Problem Details type field.
    /// Default: "https://errors.example.com/"
    /// </summary>
    public string ProblemDetailsTypeBaseUri { get; set; } = "https://errors.example.com/";

    /// <summary>
    /// Gets or sets whether to include error code in Problem Details.
    /// Default: true
    /// </summary>
    public bool IncludeErrorCode { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include error category in Problem Details extensions.
    /// Default: true
    /// </summary>
    public bool IncludeErrorCategory { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include inner errors in Problem Details extensions.
    /// Default: true
    /// </summary>
    public bool IncludeInnerErrors { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include custom extensions in Problem Details.
    /// Default: true
    /// </summary>
    public bool IncludeCustomExtensions { get; set; } = true;

    /// <summary>
    /// Gets or sets a custom status code mapper for error categories.
    /// If null, the default mapping from ErrorCategory is used.
    /// </summary>
    public Func<Error, int>? CustomStatusMapper { get; set; }
}
