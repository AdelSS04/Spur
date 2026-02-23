using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Spur.Generators;

/// <summary>
/// Helper methods for working with Roslyn syntax trees.
/// </summary>
internal static class SyntaxHelpers
{
    /// <summary>
    /// Gets the namespace of a type declaration syntax.
    /// </summary>
    public static string? GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // Try to get the namespace from a file-scoped namespace
        var fileScopedNamespace = syntax.Ancestors()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        if (fileScopedNamespace != null)
        {
            return fileScopedNamespace.Name.ToString();
        }

        // Try to get the namespace from a traditional namespace
        var namespaceDeclaration = syntax.Ancestors()
            .OfType<NamespaceDeclarationSyntax>()
            .FirstOrDefault();

        return namespaceDeclaration?.Name.ToString();
    }

    /// <summary>
    /// Determines if a type symbol is or derives from a specific type.
    /// </summary>
    public static bool IsOrDerivesFrom(ITypeSymbol? typeSymbol, string fullTypeName)
    {
        if (typeSymbol == null)
        {
            return false;
        }

        var current = typeSymbol;
        while (current != null)
        {
            if (current.ToDisplayString() == fullTypeName)
            {
                return true;
            }
            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Gets the full metadata name of a symbol including namespace.
    /// </summary>
    public static string GetFullMetadataName(ISymbol symbol)
    {
        if (symbol.ContainingNamespace == null || symbol.ContainingNamespace.IsGlobalNamespace)
        {
            return symbol.Name;
        }

        return $"{symbol.ContainingNamespace.ToDisplayString()}.{symbol.Name}";
    }
}
