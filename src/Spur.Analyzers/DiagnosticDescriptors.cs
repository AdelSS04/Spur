using Microsoft.CodeAnalysis;

namespace Spur.Analyzers;

/// <summary>
/// Diagnostic descriptors for Spur analyzers.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "Spur";

    /// <summary>
    /// RF0001: Result value is ignored.
    /// Warns when a Result-returning method is called but the result is not used.
    /// </summary>
    public static readonly DiagnosticDescriptor IgnoredResult = new(
        id: "RF0001",
        title: "Result value is ignored",
        messageFormat: "Result from '{0}' is ignored. Results should be checked for success or failure.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A method returning a Result type was called but the result was not used. This may lead to unhandled errors.");

    /// <summary>
    /// RF0002: Unsafe access to Result.Value without checking IsSuccess.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsafeValueAccess = new(
        id: "RF0002",
        title: "Unsafe access to Result.Value",
        messageFormat: "Accessing Result.Value without checking IsSuccess. This may throw an exception.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Accessing the Value property of a Result without first checking IsSuccess may result in a SpurException.");

    /// <summary>
    /// RF0003: Unsafe access to Result.Error without checking IsFailure.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsafeErrorAccess = new(
        id: "RF0003",
        title: "Unsafe access to Result.Error",
        messageFormat: "Accessing Result.Error without checking IsFailure. This may throw an exception.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Accessing the Error property of a Result without first checking IsFailure may result in a SpurException.");
}
