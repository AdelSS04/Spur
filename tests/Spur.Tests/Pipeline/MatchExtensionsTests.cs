using Xunit;
using FluentAssertions;
using Spur.Pipeline;
using Spur.Tests.Helpers;

namespace Spur.Tests.Pipeline;

public class MatchExtensionsTests
{
    // ── Sync ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Match_OnSuccess_ShouldExecuteOnSuccess()
    {
        var output = Result.Success(42).Match(v => $"OK:{v}", e => $"ERR:{e.Code}");
        output.Should().Be("OK:42");
    }

    [Fact]
    public void Match_OnFailure_ShouldExecuteOnFailure()
    {
        var output = Result.Failure<int>(Error.NotFound("nf"))
            .Match(v => $"OK:{v}", e => $"ERR:{e.Code}");
        output.Should().Be("ERR:NOT_FOUND");
    }

    [Fact]
    public void Match_DifferentReturnTypes_ShouldAllWork()
    {
        var intResult = Result.Success(42).Match(v => v * 2, _ => -1);
        intResult.Should().Be(84);

        var boolResult = Result.Success(42).Match(_ => true, _ => false);
        boolResult.Should().BeTrue();
    }

    [Fact]
    public void Match_ComplexType_ShouldWork()
    {
        var user = TestData.SampleUser;
        var result = Result.Success(user).Match(u => u.Name, e => "unknown");
        result.Should().Be("Test User");
    }

    // ── Async ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MatchAsync_OnSuccess_ShouldExecuteAsync()
    {
        var output = await Result.Success(42).MatchAsync(
            async v => { await Task.Yield(); return $"OK:{v}"; },
            async e => { await Task.Yield(); return $"ERR:{e.Code}"; });
        output.Should().Be("OK:42");
    }

    [Fact]
    public async Task MatchAsync_OnFailure_ShouldExecuteAsync()
    {
        var output = await Result.Failure<int>(Error.Conflict("c")).MatchAsync(
            async v => { await Task.Yield(); return "ok"; },
            async e => { await Task.Yield(); return $"ERR:{e.Code}"; });
        output.Should().Be("ERR:CONFLICT");
    }

    // ── Task<Result<T>> overloads ────────────────────────────────────────────

    [Fact]
    public async Task MatchAsync_TaskResult_SyncHandlers_OnSuccess()
    {
        var output = await Task.FromResult(Result.Success(10))
            .MatchAsync(v => v * 3, _ => -1);
        output.Should().Be(30);
    }

    [Fact]
    public async Task MatchAsync_TaskResult_SyncHandlers_OnFailure()
    {
        var output = await Task.FromResult(Result.Failure<int>(Error.Unauthorized("ua")))
            .MatchAsync(v => v, _ => -1);
        output.Should().Be(-1);
    }

    [Fact]
    public async Task MatchAsync_TaskResult_AsyncHandlers_OnSuccess()
    {
        var output = await Task.FromResult(Result.Success(10))
            .MatchAsync(
                async v => { await Task.Yield(); return v + 1; },
                async e => { await Task.Yield(); return -1; });
        output.Should().Be(11);
    }

    [Fact]
    public async Task MatchAsync_TaskResult_AsyncHandlers_OnFailure()
    {
        var output = await Task.FromResult(Result.Failure<int>(Error.Forbidden("f")))
            .MatchAsync(
                async v => { await Task.Yield(); return v; },
                async e => { await Task.Yield(); return e.HttpStatus; });
        output.Should().Be(403);
    }

    // ── Edge Cases ───────────────────────────────────────────────────────────

    [Fact]
    public void Match_InPipeline_ShouldWork()
    {
        var output = Result.Success(5)
            .Map(x => x * 10)
            .Match(v => $"Result: {v}", e => "Failed");
        output.Should().Be("Result: 50");
    }

    [Fact]
    public void Match_WithFailedPipeline_ShouldReturnFailureBranch()
    {
        var output = Result.Failure<int>(Error.Validation("bad"))
            .Map(x => x * 10)
            .Match(v => $"Result: {v}", e => $"Failed: {e.Code}");
        output.Should().Be("Failed: VALIDATION_ERROR");
    }
}
