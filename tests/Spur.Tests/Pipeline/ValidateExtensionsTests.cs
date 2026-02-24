using FluentAssertions;
using Spur.Pipeline;
using Xunit;

namespace Spur.Tests.Pipeline;

public class ValidateExtensionsTests
{
    private static readonly Error ValidationError = Error.Validation("Invalid", "INVALID");

    // ── Sync with static error ───────────────────────────────────────────────

    [Fact]
    public void Validate_PredicateTrue_ShouldReturnSuccess()
    {
        var result = Result.Success(10).Validate(x => x > 0, ValidationError);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void Validate_PredicateFalse_ShouldReturnError()
    {
        var result = Result.Success(-5).Validate(x => x > 0, ValidationError);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID");
    }

    [Fact]
    public void Validate_OnFailure_ShouldPropagateOriginalError()
    {
        var original = Error.NotFound("nf");
        var result = Result.Failure<int>(original).Validate(x => x > 0, ValidationError);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(original);
    }

    [Fact]
    public void Validate_NullPredicate_ShouldThrow()
    {
        var act = () => Result.Success(1).Validate(null!, ValidationError);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Sync with error factory ──────────────────────────────────────────────

    [Fact]
    public void Validate_ErrorFactory_PredicateTrue_ShouldReturnSuccess()
    {
        var result = Result.Success(10)
            .Validate(x => x > 0, x => Error.Validation($"{x} is invalid"));
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_ErrorFactory_PredicateFalse_ShouldUseFactory()
    {
        var result = Result.Success(-5)
            .Validate(x => x > 0, x => Error.Validation($"{x} must be positive"));
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("-5 must be positive");
    }

    // ── Chained validations ──────────────────────────────────────────────────

    [Fact]
    public void Validate_ChainedAll_ShouldPassWhenAllPass()
    {
        var result = Result.Success(50)
            .Validate(x => x > 0, Error.Validation("must be positive"))
            .Validate(x => x < 100, Error.Validation("must be < 100"))
            .Validate(x => x % 2 == 0, Error.Validation("must be even"));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(50);
    }

    [Fact]
    public void Validate_Chained_FailsOnFirst_ShouldShortCircuit()
    {
        bool secondChecked = false;
        var result = Result.Success(-1)
            .Validate(x => x > 0, Error.Validation("not positive"))
            .Validate(x =>
            {
                secondChecked = true;
                return x < 100;
            }, Error.Validation("too big"));

        // Short circuits: second predicate not evaluated because first fails and returns failure
        // Actually in Spur, Validate on failure propagates, so secondChecked will be false
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("not positive");
        secondChecked.Should().BeFalse();
    }

    // ── Async: Task<Result<T>> + sync predicate ─────────────────────────────

    [Fact]
    public async Task ValidateAsync_TaskResultSyncPredicate_Pass()
    {
        var result = await Task.FromResult(Result.Success(10))
            .ValidateAsync(x => x > 0, ValidationError);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_TaskResultSyncPredicate_Fail()
    {
        var result = await Task.FromResult(Result.Success(-1))
            .ValidateAsync(x => x > 0, ValidationError);
        result.IsFailure.Should().BeTrue();
    }

    // ── Async: Task<Result<T>> + async predicate ────────────────────────────

    [Fact]
    public async Task ValidateAsync_TaskResultAsyncPredicate_Pass()
    {
        var result = await Task.FromResult(Result.Success(10))
            .ValidateAsync(async x =>
            {
                await Task.Yield();
                return x > 0;
            }, ValidationError);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_TaskResultAsyncPredicate_Fail()
    {
        var result = await Task.FromResult(Result.Success(-1))
            .ValidateAsync(async x =>
            {
                await Task.Yield();
                return x > 0;
            }, ValidationError);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_TaskResultAsyncPredicateWithFactory_Fail()
    {
        var result = await Task.FromResult(Result.Success(-1))
            .ValidateAsync(
                async x => { await Task.Yield(); return x > 0; },
                x => Error.Validation($"{x} is negative"));
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("-1 is negative");
    }

    // ── Async: Result<T> + async predicate ──────────────────────────────────

    [Fact]
    public async Task ValidateAsync_ResultAsyncPredicate_Pass()
    {
        var result = await Result.Success(10).ValidateAsync(
            async x => { await Task.Yield(); return x > 0; },
            ValidationError);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ResultAsyncPredicate_Fail()
    {
        var result = await Result.Success(-1).ValidateAsync(
            async x => { await Task.Yield(); return x > 0; },
            ValidationError);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_OnFailure_ShouldNotCallPredicate()
    {
        bool called = false;
        var result = await Result.Failure<int>(Error.NotFound("nf")).ValidateAsync(
            async x => { called = true; await Task.Yield(); return true; },
            ValidationError);
        called.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }
}
