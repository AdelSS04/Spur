using Xunit;
using FluentAssertions;
using Spur.Pipeline;
using Spur.Tests.Helpers;

namespace Spur.Tests.Pipeline;

public class TapExtensionsTests
{
    // ── Tap (success side-effect) ────────────────────────────────────────────

    [Fact]
    public void Tap_OnSuccess_ShouldExecuteAction()
    {
        int captured = 0;
        var result = Result.Success(42).Tap(v => captured = v);
        captured.Should().Be(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Tap_OnFailure_ShouldNotExecuteAction()
    {
        bool executed = false;
        var result = Result.Failure<int>(TestData.Errors.NotFound).Tap(_ => executed = true);
        executed.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Tap_ShouldReturnSameResult()
    {
        var original = Result.Success(42);
        var tapped = original.Tap(_ => { });
        tapped.Value.Should().Be(original.Value);
    }

    [Fact]
    public void Tap_NullAction_ShouldThrow()
    {
        var act = () => Result.Success(1).Tap(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── TapError (failure side-effect) ───────────────────────────────────────

    [Fact]
    public void TapError_OnFailure_ShouldExecuteAction()
    {
        string? code = null;
        var result = Result.Failure<int>(Error.Conflict("c")).TapError(e => code = e.Code);
        code.Should().Be("CONFLICT");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TapError_OnSuccess_ShouldNotExecuteAction()
    {
        bool executed = false;
        var result = Result.Success(42).TapError(_ => executed = true);
        executed.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
    }

    // ── TapBoth ──────────────────────────────────────────────────────────────

    [Fact]
    public void TapBoth_OnSuccess_ShouldCallSuccessAction()
    {
        int captured = 0;
        string? errorCode = null;
        Result.Success(10).TapBoth(v => captured = v, e => errorCode = e.Code);
        captured.Should().Be(10);
        errorCode.Should().BeNull();
    }

    [Fact]
    public void TapBoth_OnFailure_ShouldCallErrorAction()
    {
        int captured = 0;
        string? errorCode = null;
        Result.Failure<int>(Error.Validation("v")).TapBoth(v => captured = v, e => errorCode = e.Code);
        captured.Should().Be(0);
        errorCode.Should().Be("VALIDATION_ERROR");
    }

    // ── TapAsync: Task<Result<T>> + sync action ─────────────────────────────

    [Fact]
    public async Task TapAsync_TaskResult_SyncAction_OnSuccess()
    {
        int captured = 0;
        var result = await Task.FromResult(Result.Success(42)).TapAsync(v => captured = v);
        captured.Should().Be(42);
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_TaskResult_SyncAction_OnFailure_ShouldNotExecute()
    {
        bool executed = false;
        var result = await Task.FromResult(Result.Failure<int>(TestData.Errors.Conflict))
            .TapAsync(_ => executed = true);
        executed.Should().BeFalse();
    }

    // ── TapAsync: Task<Result<T>> + async action ────────────────────────────

    [Fact]
    public async Task TapAsync_TaskResult_AsyncAction_OnSuccess()
    {
        int captured = 0;
        var result = await Task.FromResult(Result.Success(42))
            .TapAsync(async v => { await Task.Yield(); captured = v; });
        captured.Should().Be(42);
    }

    // ── TapErrorAsync: Task<Result<T>> + sync action ────────────────────────

    [Fact]
    public async Task TapErrorAsync_TaskResult_SyncAction_OnFailure()
    {
        string? code = null;
        var result = await Task.FromResult(Result.Failure<int>(Error.NotFound("nf")))
            .TapErrorAsync(e => code = e.Code);
        code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task TapErrorAsync_TaskResult_SyncAction_OnSuccess_ShouldNotExecute()
    {
        bool executed = false;
        var result = await Task.FromResult(Result.Success(42))
            .TapErrorAsync(_ => executed = true);
        executed.Should().BeFalse();
    }

    // ── TapErrorAsync: Task<Result<T>> + async action ───────────────────────

    [Fact]
    public async Task TapErrorAsync_TaskResult_AsyncAction_OnFailure()
    {
        string? code = null;
        var result = await Task.FromResult(Result.Failure<int>(Error.Forbidden("f")))
            .TapErrorAsync(async e => { await Task.Yield(); code = e.Code; });
        code.Should().Be("FORBIDDEN");
    }

    // ── TapAsync: Result<T> (sync) + async action ───────────────────────────

    [Fact]
    public async Task TapAsync_Result_AsyncAction_OnSuccess()
    {
        int captured = 0;
        var result = await Result.Success(55)
            .TapAsync(async v => { await Task.Yield(); captured = v; });
        captured.Should().Be(55);
    }

    [Fact]
    public async Task TapAsync_Result_AsyncAction_OnFailure_ShouldNotExecute()
    {
        bool executed = false;
        var result = await Result.Failure<int>(Error.Unauthorized("u"))
            .TapAsync(async _ => { await Task.Yield(); executed = true; });
        executed.Should().BeFalse();
    }

    // ── TapErrorAsync: Result<T> (sync) + async action ──────────────────────

    [Fact]
    public async Task TapErrorAsync_Result_AsyncAction_OnFailure()
    {
        string? code = null;
        var result = await Result.Failure<int>(Error.TooManyRequests("tmr"))
            .TapErrorAsync(async e => { await Task.Yield(); code = e.Code; });
        code.Should().Be("TOO_MANY_REQUESTS");
    }

    [Fact]
    public async Task TapErrorAsync_Result_AsyncAction_OnSuccess_ShouldNotExecute()
    {
        bool executed = false;
        var result = await Result.Success(1)
            .TapErrorAsync(async _ => { await Task.Yield(); executed = true; });
        executed.Should().BeFalse();
    }

    // ── Complex chaining ─────────────────────────────────────────────────────

    [Fact]
    public void Tap_ChainedWithMap_ShouldLogIntermediateValues()
    {
        var log = new List<string>();
        var result = Result.Success(5)
            .Tap(v => log.Add($"start:{v}"))
            .Map(x => x * 2)
            .Tap(v => log.Add($"mapped:{v}"))
            .Map(x => x.ToString());

        result.Value.Should().Be("10");
        log.Should().BeEquivalentTo(new[] { "start:5", "mapped:10" });
    }
}
