using Xunit;
using FluentAssertions;
using Spur.Pipeline;
using Spur.Tests.Helpers;

namespace Spur.Tests.Pipeline;

public class RecoverExtensionsTests
{
    // ── Recover (full) ───────────────────────────────────────────────────────

    [Fact]
    public void Recover_OnFailure_ShouldExecuteRecovery()
    {
        var result = Result.Failure<int>(Error.NotFound("nf"))
            .Recover(_ => Result.Success(42));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Recover_OnSuccess_ShouldNotExecuteRecovery()
    {
        bool executed = false;
        var result = Result.Success(42).Recover(_ =>
        {
            executed = true;
            return Result.Success(0);
        });
        executed.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Recover_WithValueFunc_OnFailure_ShouldWrapInSuccess()
    {
        var result = Result.Failure<int>(Error.NotFound("nf"))
            .Recover(e => e.HttpStatus);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(404);
    }

    [Fact]
    public void Recover_ShouldReceiveOriginalError()
    {
        Error? captured = null;
        Result.Failure<int>(Error.Conflict("conflict"))
            .Recover(e =>
            {
                captured = e;
                return Result.Success(0);
            });
        captured.Should().NotBeNull();
        captured!.Value.Code.Should().Be("CONFLICT");
    }

    [Fact]
    public void Recover_NullFunc_ShouldThrow()
    {
        var act = () => Result.Failure<int>(Error.NotFound("nf"))
            .Recover((Func<Error, Result<int>>)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── RecoverIf (category match) ──────────────────────────────────────────

    [Fact]
    public void RecoverIf_MatchingCategory_ShouldRecover()
    {
        var result = Result.Failure<int>(Error.NotFound("nf"))
            .RecoverIf(ErrorCategory.NotFound, _ => Result.Success(0));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public void RecoverIf_NonMatchingCategory_ShouldNotRecover()
    {
        var result = Result.Failure<int>(Error.NotFound("nf"))
            .RecoverIf(ErrorCategory.Validation, _ => Result.Success(0));
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void RecoverIf_OnSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success(42)
            .RecoverIf(ErrorCategory.NotFound, _ => Result.Success(0));
        result.Value.Should().Be(42);
    }

    // ── RecoverIfCode ────────────────────────────────────────────────────────

    [Fact]
    public void RecoverIfCode_MatchingCode_ShouldRecover()
    {
        var result = Result.Failure<int>(Error.NotFound("nf", "ITEM_NOT_FOUND"))
            .RecoverIfCode("ITEM_NOT_FOUND", _ => Result.Success(0));
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RecoverIfCode_NonMatchingCode_ShouldNotRecover()
    {
        var result = Result.Failure<int>(Error.NotFound("nf"))
            .RecoverIfCode("OTHER_CODE", _ => Result.Success(0));
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RecoverIfCode_OnSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success(42)
            .RecoverIfCode("NOT_FOUND", _ => Result.Success(0));
        result.Value.Should().Be(42);
    }

    // ── RecoverAsync: Task<Result<T>> ────────────────────────────────────────

    [Fact]
    public async Task RecoverAsync_TaskResult_OnFailure_ShouldRecover()
    {
        var result = await Task.FromResult(Result.Failure<int>(Error.NotFound("nf")))
            .RecoverAsync(async e =>
            {
                await Task.Yield();
                return Result.Success(0);
            });
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RecoverAsync_TaskResult_OnSuccess_ShouldReturnOriginal()
    {
        var result = await Task.FromResult(Result.Success(42))
            .RecoverAsync(async _ =>
            {
                await Task.Yield();
                return Result.Success(0);
            });
        result.Value.Should().Be(42);
    }

    // ── RecoverIfAsync: Task<Result<T>> ──────────────────────────────────────

    [Fact]
    public async Task RecoverIfAsync_Matching_ShouldRecover()
    {
        var result = await Task.FromResult(Result.Failure<int>(Error.NotFound("nf")))
            .RecoverIfAsync(ErrorCategory.NotFound, async e =>
            {
                await Task.Yield();
                return Result.Success(0);
            });
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RecoverIfAsync_NonMatching_ShouldNotRecover()
    {
        var result = await Task.FromResult(Result.Failure<int>(Error.NotFound("nf")))
            .RecoverIfAsync(ErrorCategory.Conflict, async _ =>
            {
                await Task.Yield();
                return Result.Success(0);
            });
        result.IsFailure.Should().BeTrue();
    }

    // ── RecoverAsync: Result<T> (sync) ───────────────────────────────────────

    [Fact]
    public async Task RecoverAsync_SyncResult_OnFailure_ShouldRecover()
    {
        var result = await Result.Failure<int>(Error.Validation("v"))
            .RecoverAsync(async e =>
            {
                await Task.Yield();
                return Result.Success(-1);
            });
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(-1);
    }

    [Fact]
    public async Task RecoverAsync_SyncResult_OnSuccess_ShouldReturnOriginal()
    {
        var result = await Result.Success(99)
            .RecoverAsync(async _ =>
            {
                await Task.Yield();
                return Result.Success(0);
            });
        result.Value.Should().Be(99);
    }

    // ── Complex Scenarios ────────────────────────────────────────────────────

    [Fact]
    public void Recover_ChainedRecoveryAttempts()
    {
        var result = Result.Failure<string>(Error.NotFound("nf"))
            .RecoverIf(ErrorCategory.Validation, _ => Result.Success("from-validation"))
            .RecoverIf(ErrorCategory.NotFound, _ => Result.Success("from-notfound"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("from-notfound");
    }

    [Fact]
    public void Recover_RecoveryThatAlsoFails_ShouldReturnNewFailure()
    {
        var result = Result.Failure<int>(Error.NotFound("nf"))
            .Recover(_ => Result.Failure<int>(Error.Conflict("still broken")));
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CONFLICT");
    }

    [Fact]
    public async Task ComplexPipeline_RecoverInAsyncChain()
    {
        var result = await Task.FromResult(Result.Failure<int>(Error.NotFound("nf")))
            .RecoverAsync(async _ =>
            {
                await Task.Yield();
                return Result.Success(0);
            });

        var final = result.Map(x => x + 100);
        final.IsSuccess.Should().BeTrue();
        final.Value.Should().Be(100);
    }
}
