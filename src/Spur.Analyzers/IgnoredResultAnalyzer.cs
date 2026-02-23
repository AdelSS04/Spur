using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Spur.Analyzers;

/// <summary>
/// Analyzer that detects when Result-returning methods are called but their results are ignored.
/// Diagnostic: RF0001
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IgnoredResultAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.IgnoredResult);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if this invocation is used as a statement (not assigned or returned)
        if (!IsIgnoredResult(invocation))
        {
            return;
        }

        // Get the symbol for the invoked method
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Check if the return type is Result<T>
        if (!IsResultType(methodSymbol.ReturnType))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.IgnoredResult,
            invocation.GetLocation(),
            methodSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsIgnoredResult(InvocationExpressionSyntax invocation)
    {
        // Check if the invocation is part of an expression statement (i.e., not assigned or returned)
        var parent = invocation.Parent;

        return parent is ExpressionStatementSyntax;
    }

    private static bool IsResultType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
        {
            return false;
        }

        // Check if it's Result<T>
        if (namedType.IsGenericType &&
            namedType.ConstructedFrom.ToDisplayString() == "Spur.Result<T>")
        {
            return true;
        }

        // Check the full name
        var fullName = namedType.OriginalDefinition.ToDisplayString();
        return fullName.StartsWith("Spur.Result<");
    }
}
