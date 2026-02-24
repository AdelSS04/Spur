using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Xunit;

namespace Spur.Analyzers.Tests;

public static class AnalyzerTestHelper
{
    public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(
        string source,
        params DiagnosticAnalyzer[] analyzers)
    {
        var spurReference = MetadataReference.CreateFromFile(typeof(Result<>).Assembly.Location);
        var runtimeReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var systemRuntimeReference = MetadataReference.CreateFromFile(
            typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location);
        var taskReference = MetadataReference.CreateFromFile(typeof(Task).Assembly.Location);
        var linqReference = MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location);

        // Get the System.Runtime reference that contains basic types
        var systemRuntime = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            systemRuntime,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create(analyzers));

        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        return diagnostics;
    }
}

// ── IgnoredResultAnalyzer Tests ──────────────────────────────────────────────

public class IgnoredResultAnalyzerTests
{
    private readonly IgnoredResultAnalyzer _analyzer = new();

    [Fact]
    public void SupportedDiagnostics_ShouldContainRF0001()
    {
        _analyzer.SupportedDiagnostics.Should().ContainSingle(d => d.Id == "RF0001");
    }

    [Fact]
    public async Task IgnoredResult_ShouldReportDiagnostic()
    {
        var source = @"
using Spur;

public class TestClass
{
    public void TestMethod()
    {
        GetResult();
    }

    private Result<int> GetResult() => Result.Success(42);
}";

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source, _analyzer);
        diagnostics.Should().Contain(d => d.Id == "RF0001");
    }

    [Fact]
    public async Task AssignedResult_ShouldNotReportDiagnostic()
    {
        var source = @"
using Spur;

public class TestClass
{
    public void TestMethod()
    {
        var result = GetResult();
    }

    private Result<int> GetResult() => Result.Success(42);
}";

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source, _analyzer);
        diagnostics.Where(d => d.Id == "RF0001").Should().BeEmpty();
    }

    [Fact]
    public async Task ReturnedResult_ShouldNotReportDiagnostic()
    {
        var source = @"
using Spur;

public class TestClass
{
    public Result<int> TestMethod()
    {
        return GetResult();
    }

    private Result<int> GetResult() => Result.Success(42);
}";

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source, _analyzer);
        diagnostics.Where(d => d.Id == "RF0001").Should().BeEmpty();
    }

    [Fact]
    public async Task NonResultMethod_ShouldNotReportDiagnostic()
    {
        var source = @"
public class TestClass
{
    public void TestMethod()
    {
        GetValue();
    }

    private int GetValue() => 42;
}";

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source, _analyzer);
        diagnostics.Where(d => d.Id == "RF0001").Should().BeEmpty();
    }
}

// ── UnsafeValueAccessAnalyzer Tests ──────────────────────────────────────────

public class UnsafeValueAccessAnalyzerTests
{
    private readonly UnsafeValueAccessAnalyzer _analyzer = new();

    [Fact]
    public void SupportedDiagnostics_ShouldContainRF0002AndRF0003()
    {
        _analyzer.SupportedDiagnostics.Should().Contain(d => d.Id == "RF0002");
        _analyzer.SupportedDiagnostics.Should().Contain(d => d.Id == "RF0003");
    }

    [Fact]
    public async Task UnsafeValueAccess_ShouldReportRF0002()
    {
        var source = @"
using Spur;

public class TestClass
{
    public void TestMethod()
    {
        var result = Result.Success(42);
        var value = result.Value;
    }
}";

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source, _analyzer);
        diagnostics.Should().Contain(d => d.Id == "RF0002");
    }

    [Fact]
    public async Task SafeValueAccess_WithIsSuccessCheck_ShouldNotReport()
    {
        var source = @"
using Spur;

public class TestClass
{
    public void TestMethod()
    {
        var result = Result.Success(42);
        if (result.IsSuccess)
        {
            var value = result.Value;
        }
    }
}";

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source, _analyzer);
        diagnostics.Where(d => d.Id == "RF0002").Should().BeEmpty();
    }

    [Fact]
    public async Task UnsafeErrorAccess_ShouldReportRF0003()
    {
        var source = @"
using Spur;

public class TestClass
{
    public void TestMethod()
    {
        var result = Result.Failure<int>(Error.NotFound(""nf""));
        var error = result.Error;
    }
}";

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source, _analyzer);
        diagnostics.Should().Contain(d => d.Id == "RF0003");
    }

    [Fact]
    public async Task SafeErrorAccess_WithIsFailureCheck_ShouldNotReport()
    {
        var source = @"
using Spur;

public class TestClass
{
    public void TestMethod()
    {
        var result = Result.Failure<int>(Error.NotFound(""nf""));
        if (result.IsFailure)
        {
            var error = result.Error;
        }
    }
}";

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source, _analyzer);
        diagnostics.Where(d => d.Id == "RF0003").Should().BeEmpty();
    }

    [Fact]
    public async Task TernaryGuard_ShouldNotReport()
    {
        var source = @"
using Spur;

public class TestClass
{
    public int TestMethod()
    {
        var result = Result.Success(42);
        return result.IsSuccess ? result.Value : 0;
    }
}";

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source, _analyzer);
        diagnostics.Where(d => d.Id == "RF0002").Should().BeEmpty();
    }
}

// ── DiagnosticDescriptors Tests ──────────────────────────────────────────────

public class DiagnosticDescriptorsTests
{
    [Fact]
    public void IgnoredResult_ShouldHaveCorrectProperties()
    {
        DiagnosticDescriptors.IgnoredResult.Id.Should().Be("RF0001");
        DiagnosticDescriptors.IgnoredResult.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
        DiagnosticDescriptors.IgnoredResult.IsEnabledByDefault.Should().BeTrue();
    }

    [Fact]
    public void UnsafeValueAccess_ShouldHaveCorrectProperties()
    {
        DiagnosticDescriptors.UnsafeValueAccess.Id.Should().Be("RF0002");
        DiagnosticDescriptors.UnsafeValueAccess.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
        DiagnosticDescriptors.UnsafeValueAccess.IsEnabledByDefault.Should().BeTrue();
    }

    [Fact]
    public void UnsafeErrorAccess_ShouldHaveCorrectProperties()
    {
        DiagnosticDescriptors.UnsafeErrorAccess.Id.Should().Be("RF0003");
        DiagnosticDescriptors.UnsafeErrorAccess.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
        DiagnosticDescriptors.UnsafeErrorAccess.IsEnabledByDefault.Should().BeTrue();
    }
}
