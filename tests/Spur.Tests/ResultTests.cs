using Xunit;
using FluentAssertions;

namespace Spur.Tests;

public class ResultTests
{
    // ── Success / Failure Creation ───────────────────────────────────────────

    [Fact]
    public void Success_Int_ShouldCreateSuccessResult()
    {
        var result = Result.Success(42);
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Success_String_ShouldCreateSuccessResult()
    {
        var result = Result.Success("hello");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Success_Unit_ShouldCreateSuccessResult()
    {
        var result = Result.Success();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        var error = Error.NotFound("Not found");
        var result = Result.Failure<int>(error);
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WithCodeMessageStatus_ShouldCreateFailure()
    {
        var result = Result.Failure<string>("CUSTOM", "Custom msg", 418);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CUSTOM");
        result.Error.Message.Should().Be("Custom msg");
        result.Error.HttpStatus.Should().Be(418);
    }

    // ── Value / Error Access ─────────────────────────────────────────────────

    [Fact]
    public void Value_OnFailure_ShouldThrowInvalidOperationException()
    {
        var result = Result.Failure<int>(Error.NotFound("Not found"));
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot access Value*");
    }

    [Fact]
    public void Error_OnSuccess_ShouldThrowInvalidOperationException()
    {
        var result = Result.Success(42);
        var act = () => result.Error;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot access Error*");
    }

    [Fact]
    public void GetValueOrDefault_OnSuccess_ShouldReturnValue()
    {
        Result.Success(42).GetValueOrDefault().Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ShouldReturnDefault()
    {
        Result.Failure<int>(Error.NotFound("x")).GetValueOrDefault().Should().Be(0);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_StringShouldReturnNull()
    {
        Result.Failure<string>(Error.NotFound("x")).GetValueOrDefault().Should().BeNull();
    }

    [Fact]
    public void GetErrorOrDefault_OnFailure_ShouldReturnError()
    {
        var error = Error.NotFound("x");
        Result.Failure<int>(error).GetErrorOrDefault().Should().Be(error);
    }

    [Fact]
    public void GetErrorOrDefault_OnSuccess_ShouldReturnDefaultError()
    {
        var err = Result.Success(42).GetErrorOrDefault();
        err.Code.Should().BeNullOrEmpty();
    }

    // ── Implicit Conversions ─────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        Result<int> result = 42;
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        var error = Error.NotFound("Not found");
        Result<int> result = error;
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromString_ShouldCreateSuccess()
    {
        Result<string> result = "hello";
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    // ── Match ────────────────────────────────────────────────────────────────

    [Fact]
    public void Match_OnSuccess_ShouldExecuteOnSuccess()
    {
        var result = Result.Success(42);
        var output = result.Match(v => $"OK:{v}", e => $"ERR:{e.Code}");
        output.Should().Be("OK:42");
    }

    [Fact]
    public void Match_OnFailure_ShouldExecuteOnFailure()
    {
        var result = Result.Failure<int>(Error.NotFound("nf"));
        var output = result.Match(v => $"OK:{v}", e => $"ERR:{e.Code}");
        output.Should().Be("ERR:NOT_FOUND");
    }

    [Fact]
    public async Task MatchAsync_OnSuccess_ShouldExecuteAsync()
    {
        var result = Result.Success(10);
        var output = await result.MatchAsync(
            async v => { await Task.Yield(); return v * 2; },
            async e => { await Task.Yield(); return -1; });
        output.Should().Be(20);
    }

    [Fact]
    public async Task MatchAsync_OnFailure_ShouldExecuteAsync()
    {
        var result = Result.Failure<int>(Error.NotFound("x"));
        var output = await result.MatchAsync(
            async v => { await Task.Yield(); return v; },
            async e => { await Task.Yield(); return -1; });
        output.Should().Be(-1);
    }

    // ── Switch ───────────────────────────────────────────────────────────────

    [Fact]
    public void Switch_OnSuccess_ShouldInvokeOnSuccess()
    {
        var result = Result.Success(42);
        int captured = 0;
        result.Switch(v => captured = v, _ => { });
        captured.Should().Be(42);
    }

    [Fact]
    public void Switch_OnFailure_ShouldInvokeOnFailure()
    {
        var result = Result.Failure<int>(Error.Conflict("c"));
        string? code = null;
        result.Switch(_ => { }, e => code = e.Code);
        code.Should().Be("CONFLICT");
    }

    [Fact]
    public async Task SwitchAsync_OnSuccess_ShouldInvokeAsync()
    {
        var result = Result.Success(5);
        int captured = 0;
        await result.SwitchAsync(
            async v => { await Task.Yield(); captured = v; },
            async e => { await Task.Yield(); });
        captured.Should().Be(5);
    }

    [Fact]
    public async Task SwitchAsync_OnFailure_ShouldInvokeAsync()
    {
        var result = Result.Failure<int>(Error.Forbidden("f"));
        string? code = null;
        await result.SwitchAsync(
            async _ => { await Task.Yield(); },
            async e => { await Task.Yield(); code = e.Code; });
        code.Should().Be("FORBIDDEN");
    }

    // ── Unwrap ───────────────────────────────────────────────────────────────

    [Fact]
    public void Unwrap_OnSuccess_ShouldReturnValue()
    {
        Result.Success(42).Unwrap().Should().Be(42);
    }

    [Fact]
    public void Unwrap_OnFailure_ShouldThrowSpurException()
    {
        var result = Result.Failure<int>(Error.NotFound("nf"));
        var act = () => result.Unwrap();
        act.Should().Throw<SpurException>()
            .Which.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void UnwrapOr_OnSuccess_ShouldReturnValue()
    {
        Result.Success(42).UnwrapOr(0).Should().Be(42);
    }

    [Fact]
    public void UnwrapOr_OnFailure_ShouldReturnFallback()
    {
        Result.Failure<int>(Error.NotFound("x")).UnwrapOr(99).Should().Be(99);
    }

    [Fact]
    public void UnwrapOrElse_OnSuccess_ShouldReturnValue()
    {
        Result.Success(42).UnwrapOrElse(_ => 0).Should().Be(42);
    }

    [Fact]
    public void UnwrapOrElse_OnFailure_ShouldInvokeFactory()
    {
        var result = Result.Failure<int>(Error.NotFound("x"));
        result.UnwrapOrElse(e => e.HttpStatus).Should().Be(404);
    }

    // ── Start ────────────────────────────────────────────────────────────────

    [Fact]
    public void Start_ShouldCreateSuccessResult()
    {
        var result = Result.Start(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task StartAsync_WithValueFactory_ShouldCreateSuccess()
    {
        var result = await Result.StartAsync(() => Task.FromResult(42));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task StartAsync_WithResultFactory_ShouldReturnResult()
    {
        var result = await Result.StartAsync(() => Task.FromResult(Result.Success(42)));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task StartAsync_WithResultFactory_ReturningFailure_ShouldReturnFailure()
    {
        var result = await Result.StartAsync(() =>
            Task.FromResult(Result.Failure<int>(Error.NotFound("x"))));
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NOT_FOUND");
    }

    // ── Try ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Try_Success_ShouldReturnSuccessResult()
    {
        var result = Result.Try(() => 42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Try_Exception_ShouldReturnUnexpectedError()
    {
        var result = Result.Try<int>(() => throw new InvalidOperationException("Boom"));
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.Unexpected);
        result.Error.Message.Should().Contain("Boom");
    }

    [Fact]
    public async Task TryAsync_Success_ShouldReturnSuccessResult()
    {
        var result = await Result.TryAsync(async () =>
        {
            await Task.Yield();
            return 42;
        });
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TryAsync_Exception_ShouldReturnUnexpectedError()
    {
        var result = await Result.TryAsync<int>(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Async boom");
        });
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.Unexpected);
        result.Error.Message.Should().Contain("Async boom");
    }

    // ── Combine ──────────────────────────────────────────────────────────────

    [Fact]
    public void Combine_TwoSuccess_ShouldReturnTuple()
    {
        var r1 = Result.Success(1);
        var r2 = Result.Success("two");
        var combined = Result.Combine(r1, r2);
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, "two"));
    }

    [Fact]
    public void Combine_TwoWithFirstFailure_ShouldReturnFirstError()
    {
        var r1 = Result.Failure<int>(Error.NotFound("nf"));
        var r2 = Result.Success("two");
        var combined = Result.Combine(r1, r2);
        combined.IsFailure.Should().BeTrue();
        combined.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void Combine_TwoWithSecondFailure_ShouldReturnSecondError()
    {
        var r1 = Result.Success(1);
        var r2 = Result.Failure<string>(Error.Conflict("c"));
        var combined = Result.Combine(r1, r2);
        combined.IsFailure.Should().BeTrue();
        combined.Error.Code.Should().Be("CONFLICT");
    }

    [Fact]
    public void Combine_ThreeSuccess_ShouldReturnTuple()
    {
        var combined = Result.Combine(
            Result.Success(1),
            Result.Success("two"),
            Result.Success(3.0));
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, "two", 3.0));
    }

    [Fact]
    public void Combine_ThreeWithMiddleFailure_ShouldReturnFirstError()
    {
        var combined = Result.Combine(
            Result.Success(1),
            Result.Failure<string>(Error.Validation("bad")),
            Result.Success(3.0));
        combined.IsFailure.Should().BeTrue();
        combined.Error.Code.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public void Combine_Params_AllSuccess_ShouldReturnSuccessArray()
    {
        var combined = Result.Combine(
            Result.Success(1),
            Result.Success(2),
            Result.Success(3));
        combined.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Combine_Params_AnyFailure_ShouldReturnFirstFailure()
    {
        var combined = Result.Combine(
            Result.Success(1),
            Result.Failure<int>(Error.NotFound("nf")),
            Result.Failure<int>(Error.Validation("v")));
        combined.IsFailure.Should().BeTrue();
        combined.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void CombineAll_AllSuccess_ShouldReturnAllValues()
    {
        var combined = Result.CombineAll(
            Result.Success(10),
            Result.Success(20),
            Result.Success(30));
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().BeEquivalentTo(new[] { 10, 20, 30 });
    }

    [Fact]
    public void CombineAll_AnyFailure_ShouldReturnAggregatedError()
    {
        var combined = Result.CombineAll(
            Result.Success(10),
            Result.Failure<int>(Error.Conflict("c")),
            Result.Success(30));
        combined.IsFailure.Should().BeTrue();
        combined.Error.Code.Should().Be("MULTIPLE_ERRORS");
        combined.Error.Message.Should().Contain("1 error(s) occurred.");
        combined.Error.Extensions.Should().ContainKey("errors");
    }

    // ── Complex Integration Scenarios ────────────────────────────────────────

    [Fact]
    public void Result_ShouldWorkWithRecordTypes()
    {
        var user = new TestUser(1, "Alice", "alice@test.com");
        var result = Result.Success(user);
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Alice");
    }

    [Fact]
    public void Result_FailureThenMatch_ShouldProduceFallback()
    {
        var result = Result.Failure<int>(Error.Unauthorized("no auth"));
        var msg = result.Match(_ => "OK", e => $"Error: {e.HttpStatus}");
        msg.Should().Be("Error: 401");
    }

    [Fact]
    public void Try_NestedExceptions_ShouldCaptureInnermost()
    {
        var result = Result.Try<int>(() =>
            throw new AggregateException("Outer",
                new InvalidOperationException("Inner")));

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Outer");
    }

    [Fact]
    public async Task FullPipeline_StartTryMatchAsync()
    {
        var output = await Result.Start(10)
            .Match(
                v => $"Value: {v}",
                e => $"Error: {e.Code}")
            .AsTask();

        output.Should().Be("Value: 10");
    }

    [Fact]
    public void MultipleFailures_CombineAll_ShouldAggregateAllErrors()
    {
        var results = Enumerable.Range(0, 5)
            .Select(i => i == 2 || i == 4
                ? Result.Failure<int>(Error.Validation($"Error at {i}"))
                : Result.Success(i))
            .ToArray();

        var combined = Result.CombineAll(results);
        combined.IsFailure.Should().BeTrue();
        combined.Error.Code.Should().Be("MULTIPLE_ERRORS");
        combined.Error.Message.Should().Be("2 error(s) occurred.");
        combined.Error.Extensions.Should().ContainKey("errors");
    }

    private record TestUser(int Id, string Name, string Email);
}

internal static class TaskExtensions
{
    public static Task<T> AsTask<T>(this T value) => Task.FromResult(value);
}
