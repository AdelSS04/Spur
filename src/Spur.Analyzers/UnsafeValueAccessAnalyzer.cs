using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Spur.Analyzers;

/// <summary>
/// Analyzer that detects unsafe access to Result.Value or Result.Error without proper guards.
/// Diagnostics: RF0002, RF0003
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnsafeValueAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticDescriptors.UnsafeValueAccess,
            DiagnosticDescriptors.UnsafeErrorAccess);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;

        // Get the symbol for the accessed member
        var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
        if (symbolInfo.Symbol is not IPropertySymbol propertySymbol)
        {
            return;
        }

        // Check if accessing Value or Error property
        var propertyName = propertySymbol.Name;
        if (propertyName != "Value" && propertyName != "Error")
        {
            return;
        }

        // Check if the containing type is Result<T>
        var containingType = propertySymbol.ContainingType;
        if (!IsResultType(containingType))
        {
            return;
        }

        // Check if there's a guard (IsSuccess or IsFailure check)
        if (HasProperGuard(memberAccess, propertyName, context))
        {
            return;
        }

        // Report the appropriate diagnostic
        var descriptor = propertyName == "Value"
            ? DiagnosticDescriptors.UnsafeValueAccess
            : DiagnosticDescriptors.UnsafeErrorAccess;

        var diagnostic = Diagnostic.Create(descriptor, memberAccess.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsResultType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
        {
            return false;
        }

        var fullName = namedType.OriginalDefinition.ToDisplayString();
        return fullName.StartsWith("Spur.Result<");
    }

    private static bool HasProperGuard(
        MemberAccessExpressionSyntax memberAccess,
        string propertyName,
        SyntaxNodeAnalysisContext context)
    {
        // Look for an if statement that checks IsSuccess or IsFailure
        var currentNode = memberAccess.Parent;

        while (currentNode != null)
        {
            if (currentNode is IfStatementSyntax ifStatement)
            {
                // Check if the condition checks IsSuccess/IsFailure
                var guardProperty = propertyName == "Value" ? "IsSuccess" : "IsFailure";
                if (ContainsGuardCheck(ifStatement.Condition, guardProperty, context))
                {
                    return true;
                }
            }

            // Also check for ternary expressions
            if (currentNode is ConditionalExpressionSyntax conditional)
            {
                var guardProperty = propertyName == "Value" ? "IsSuccess" : "IsFailure";
                if (ContainsGuardCheck(conditional.Condition, guardProperty, context))
                {
                    return true;
                }
            }

            currentNode = currentNode.Parent;
        }

        return false;
    }

    private static bool ContainsGuardCheck(
        ExpressionSyntax condition,
        string guardProperty,
        SyntaxNodeAnalysisContext context)
    {
        // Simple check: does the condition contain IsSuccess or IsFailure?
        var conditionText = condition.ToString();
        return conditionText.Contains(guardProperty);
    }
}
