using FluentAssertions;
using Spur.Pipeline;
using Xunit;

namespace Spur.Tests.Pipeline;

public class ThenExtensionsTests
{
    // ── Sync ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Then_OnSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success(10).Then(x => Result.Success(x * 2));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public void Then_OnFailure_ShouldNotExecuteFunction()
    {
        var error = Error.NotFound("Not found");
        bool executed = false;
        var result = Result.Failure<int>(error).Then(x =>
        {
            executed = true;
            return Result.Success(x * 2);
        });
        executed.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Then_FunctionReturnsFailure_ShouldPropagateNewFailure()
    {
        var result = Result.Success(10)
            .Then(_ => Result.Failure<int>(Error.Validation("Invalid")));
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public void Then_NullFunc_ShouldThrowArgumentNullException()
    {
        var act = () => Result.Success(1).Then<int, int>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Then_Chain_ShouldComposeOperations()
    {
        var result = Result.Success(5)
            .Then(x => x > 0 ? Result.Success(x * 10) : Result.Failure<int>(Error.Validation("neg")))
            .Then(x => Result.Success(x.ToString()));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("50");
    }

    [Fact]
    public void Then_Chain_WithEarlyFailure_ShouldShortCircuit()
    {
        bool secondCalled = false;
        var result = Result.Success(5)
            .Then(_ => Result.Failure<int>(Error.Validation("stop")))
            .Then(x =>
            {
                secondCalled = true;
                return Result.Success(x);
            });
        secondCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    // ── Then with null checking (class constraint) ───────────────────────────

    [Fact]
    public void Then_NullCheckClass_WhenNull_ShouldReturnError()
    {
        var result = Result.Success(10)
            .Then<int, string>(x => (string?)null, Error.DefaultNotFound);
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public void Then_NullCheckClass_WhenNonNull_ShouldReturnSuccess()
    {
        var result = Result.Success(10)
            .Then<int, string>(x => "value", Error.DefaultNotFound);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value");
    }

    // ── Then with null checking (struct constraint) ──────────────────────────

    [Fact]
    public void Then_NullCheckStruct_WhenNull_ShouldReturnError()
    {
        var result = Result.Success(10)
            .Then<int, int>(x => (int?)null, Error.DefaultNotFound);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Then_NullCheckStruct_WhenNonNull_ShouldReturnSuccess()
    {
        var result = Result.Success(10)
            .Then<int, int>(x => (int?)42, Error.DefaultNotFound);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    // ── Async ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ThenAsync_ResultToAsyncFunc_OnSuccess()
    {
        var result = await Result.Success(10).ThenAsync(async x =>
        {
            await Task.Yield();
            return Result.Success(x * 3);
        });
        result.Value.Should().Be(30);
    }

    [Fact]
    public async Task ThenAsync_ResultToAsyncFunc_OnFailure_ShouldNotExecute()
    {
        bool executed = false;
        var result = await Result.Failure<int>(Error.NotFound("nf")).ThenAsync(async x =>
        {
            executed = true;
            await Task.Yield();
            return Result.Success(x);
        });
        executed.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ThenAsync_TaskResultToSyncFunc_ShouldChain()
    {
        var result = await Task.FromResult(Result.Success(10))
            .ThenAsync(x => Result.Success(x * 2));
        result.Value.Should().Be(20);
    }

    [Fact]
    public async Task ThenAsync_TaskResultToAsyncFunc_ShouldChain()
    {
        var result = await Task.FromResult(Result.Success(10))
            .ThenAsync(async x =>
            {
                await Task.Yield();
                return Result.Success(x + 5);
            });
        result.Value.Should().Be(15);
    }

    [Fact]
    public async Task ThenAsync_TaskResultWithNullCheck_WhenNull_ShouldFail()
    {
        var result = await Task.FromResult(Result.Success(10))
            .ThenAsync<int, string>(async x =>
            {
                await Task.Yield();
                return (string?)null;
            }, Error.DefaultNotFound);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ThenAsync_TaskResultWithNullCheck_WhenNonNull_ShouldSucceed()
    {
        var result = await Task.FromResult(Result.Success(10))
            .ThenAsync<int, string>(async x =>
            {
                await Task.Yield();
                return "found";
            }, Error.DefaultNotFound);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("found");
    }

    // ── Complex Scenario ─────────────────────────────────────────────────────

    [Fact]
    public async Task ComplexPipeline_ThenChainWithMixedSyncAsync()
    {
        var result = await Result.Start(100)
            .Then(x => Result.Success(x / 2))
            .ThenAsync(async x =>
            {
                await Task.Yield();
                return x > 10 ? Result.Success(x.ToString()) : Result.Failure<string>(Error.Validation("too small"));
            });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("50");
    }
}
