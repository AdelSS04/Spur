using FluentAssertions;
using Xunit;

namespace Spur.Testing.Tests;

public class ResultAssertionsTests
{
    // ── ShouldBeSuccess ──────────────────────────────────────────────────────

    [Fact]
    public void ShouldBeSuccess_OnSuccess_ShouldNotThrow()
    {
        var result = Result.Success(42);
        var context = result.ShouldBeSuccess();
        context.Value.Should().Be(42);
    }

    [Fact]
    public void ShouldBeSuccess_OnFailure_ShouldThrowAssertionException()
    {
        var result = Result.Failure<int>(Error.NotFound("Not found"));
        var act = () => result.ShouldBeSuccess();
        act.Should().Throw<ResultAssertionException>()
            .WithMessage("*Expected result to be successful*");
    }

    [Fact]
    public void ShouldBeSuccess_WithValueAssertion_ShouldInvokeAssertion()
    {
        var result = Result.Success(42);
        int captured = 0;
        result.ShouldBeSuccess(v => captured = v);
        captured.Should().Be(42);
    }

    [Fact]
    public void ShouldBeSuccess_WithFailingValueAssertion_ShouldThrow()
    {
        var result = Result.Success(42);
        var act = () => result.ShouldBeSuccess(v =>
        {
            if (v != 100) throw new Exception("Wrong value");
        });
        act.Should().Throw<ResultAssertionException>()
            .WithMessage("*Value assertion failed*");
    }

    // ── ShouldBeFailure ──────────────────────────────────────────────────────

    [Fact]
    public void ShouldBeFailure_OnFailure_ShouldNotThrow()
    {
        var result = Result.Failure<int>(Error.NotFound("Not found"));
        var context = result.ShouldBeFailure();
        context.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void ShouldBeFailure_OnSuccess_ShouldThrowAssertionException()
    {
        var result = Result.Success(42);
        var act = () => result.ShouldBeFailure();
        act.Should().Throw<ResultAssertionException>()
            .WithMessage("*Expected result to be a failure*");
    }

    // ── ShouldBeFailureWithCode ──────────────────────────────────────────────

    [Fact]
    public void ShouldBeFailureWithCode_MatchingCode_ShouldNotThrow()
    {
        var result = Result.Failure<int>(Error.NotFound("nf", "USER_NOT_FOUND"));
        result.ShouldBeFailureWithCode("USER_NOT_FOUND");
    }

    [Fact]
    public void ShouldBeFailureWithCode_NonMatchingCode_ShouldThrow()
    {
        var result = Result.Failure<int>(Error.NotFound("nf"));
        var act = () => result.ShouldBeFailureWithCode("WRONG_CODE");
        act.Should().Throw<ResultAssertionException>()
            .WithMessage("*Expected error code*");
    }

    // ── ShouldBeFailureWithStatus ────────────────────────────────────────────

    [Fact]
    public void ShouldBeFailureWithStatus_MatchingStatus_ShouldNotThrow()
    {
        var result = Result.Failure<int>(Error.NotFound("nf"));
        result.ShouldBeFailureWithStatus(404);
    }

    [Fact]
    public void ShouldBeFailureWithStatus_NonMatchingStatus_ShouldThrow()
    {
        var result = Result.Failure<int>(Error.NotFound("nf"));
        var act = () => result.ShouldBeFailureWithStatus(500);
        act.Should().Throw<ResultAssertionException>()
            .WithMessage("*Expected HTTP status*");
    }

    // ── ShouldBeFailureWithCategory ──────────────────────────────────────────

    [Fact]
    public void ShouldBeFailureWithCategory_MatchingCategory_ShouldNotThrow()
    {
        var result = Result.Failure<int>(Error.Validation("v"));
        result.ShouldBeFailureWithCategory(ErrorCategory.Validation);
    }

    [Fact]
    public void ShouldBeFailureWithCategory_NonMatchingCategory_ShouldThrow()
    {
        var result = Result.Failure<int>(Error.Validation("v"));
        var act = () => result.ShouldBeFailureWithCategory(ErrorCategory.NotFound);
        act.Should().Throw<ResultAssertionException>()
            .WithMessage("*Expected error category*");
    }

    // ── Async Assertions ─────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldBeSuccessAsync_OnSuccess_ShouldNotThrow()
    {
        var resultTask = Task.FromResult(Result.Success(42));
        var context = await resultTask.ShouldBeSuccessAsync();
        context.Value.Should().Be(42);
    }

    [Fact]
    public async Task ShouldBeSuccessAsync_OnFailure_ShouldThrow()
    {
        var resultTask = Task.FromResult(Result.Failure<int>(Error.NotFound("nf")));
        var act = async () => await resultTask.ShouldBeSuccessAsync();
        await act.Should().ThrowAsync<ResultAssertionException>();
    }

    [Fact]
    public async Task ShouldBeSuccessAsync_WithAssertion_ShouldWork()
    {
        var resultTask = Task.FromResult(Result.Success(42));
        int captured = 0;
        await resultTask.ShouldBeSuccessAsync(v => captured = v);
        captured.Should().Be(42);
    }

    [Fact]
    public async Task ShouldBeFailureAsync_OnFailure_ShouldNotThrow()
    {
        var resultTask = Task.FromResult(Result.Failure<int>(Error.Conflict("c")));
        var context = await resultTask.ShouldBeFailureAsync();
        context.Error.Code.Should().Be("CONFLICT");
    }

    [Fact]
    public async Task ShouldBeFailureAsync_OnSuccess_ShouldThrow()
    {
        var resultTask = Task.FromResult(Result.Success(42));
        var act = async () => await resultTask.ShouldBeFailureAsync();
        await act.Should().ThrowAsync<ResultAssertionException>();
    }

    [Fact]
    public async Task ShouldBeFailureWithCodeAsync_MatchingCode_ShouldNotThrow()
    {
        var resultTask = Task.FromResult(Result.Failure<int>(Error.NotFound("nf")));
        var context = await resultTask.ShouldBeFailureWithCodeAsync("NOT_FOUND");
        context.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task ShouldBeFailureWithStatusAsync_MatchingStatus_ShouldNotThrow()
    {
        var resultTask = Task.FromResult(Result.Failure<int>(Error.Unauthorized("ua")));
        var context = await resultTask.ShouldBeFailureWithStatusAsync(401);
        context.Error.HttpStatus.Should().Be(401);
    }
}

// ── SuccessResultContext Tests ────────────────────────────────────────────────

public class SuccessResultContextTests
{
    [Fact]
    public void Value_ShouldReturnTheValue()
    {
        var ctx = Result.Success(42).ShouldBeSuccess();
        ctx.Value.Should().Be(42);
    }

    [Fact]
    public void WithValue_Action_ShouldInvokeAndReturnContext()
    {
        int captured = 0;
        var ctx = Result.Success(42).ShouldBeSuccess()
            .WithValue(v => captured = v);
        captured.Should().Be(42);
        ctx.Value.Should().Be(42);
    }

    [Fact]
    public void WithValue_Expected_Matching_ShouldNotThrow()
    {
        Result.Success(42).ShouldBeSuccess().WithValue(42);
    }

    [Fact]
    public void WithValue_Expected_NonMatching_ShouldThrow()
    {
        var act = () => Result.Success(42).ShouldBeSuccess().WithValue(99);
        act.Should().Throw<ResultAssertionException>()
            .WithMessage("*Expected value to be 99*");
    }

    [Fact]
    public void WithValue_ChainedAssertions()
    {
        var values = new List<int>();
        Result.Success(42).ShouldBeSuccess()
            .WithValue(v => values.Add(v))
            .WithValue(v => values.Add(v * 2));
        values.Should().BeEquivalentTo(new[] { 42, 84 });
    }
}

// ── FailureResultContext Tests ────────────────────────────────────────────────

public class FailureResultContextTests
{
    [Fact]
    public void Error_ShouldReturnTheError()
    {
        var ctx = Result.Failure<int>(Error.NotFound("nf")).ShouldBeFailure();
        ctx.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void WithCode_Matching_ShouldNotThrow()
    {
        Result.Failure<int>(Error.NotFound("nf")).ShouldBeFailure()
            .WithCode("NOT_FOUND");
    }

    [Fact]
    public void WithCode_NonMatching_ShouldThrow()
    {
        var act = () => Result.Failure<int>(Error.NotFound("nf")).ShouldBeFailure()
            .WithCode("WRONG");
        act.Should().Throw<ResultAssertionException>();
    }

    [Fact]
    public void WithHttpStatus_Matching_ShouldNotThrow()
    {
        Result.Failure<int>(Error.NotFound("nf")).ShouldBeFailure()
            .WithHttpStatus(404);
    }

    [Fact]
    public void WithHttpStatus_NonMatching_ShouldThrow()
    {
        var act = () => Result.Failure<int>(Error.NotFound("nf")).ShouldBeFailure()
            .WithHttpStatus(500);
        act.Should().Throw<ResultAssertionException>();
    }

    [Fact]
    public void WithCategory_Matching_ShouldNotThrow()
    {
        Result.Failure<int>(Error.Validation("v")).ShouldBeFailure()
            .WithCategory(ErrorCategory.Validation);
    }

    [Fact]
    public void WithCategory_NonMatching_ShouldThrow()
    {
        var act = () => Result.Failure<int>(Error.Validation("v")).ShouldBeFailure()
            .WithCategory(ErrorCategory.NotFound);
        act.Should().Throw<ResultAssertionException>();
    }

    [Fact]
    public void WithMessage_Matching_ShouldNotThrow()
    {
        Result.Failure<int>(Error.Validation("Exact message")).ShouldBeFailure()
            .WithMessage("Exact message");
    }

    [Fact]
    public void WithMessage_NonMatching_ShouldThrow()
    {
        var act = () => Result.Failure<int>(Error.Validation("A")).ShouldBeFailure()
            .WithMessage("B");
        act.Should().Throw<ResultAssertionException>();
    }

    [Fact]
    public void WithMessageContaining_Matching_ShouldNotThrow()
    {
        Result.Failure<int>(Error.Validation("Name is required")).ShouldBeFailure()
            .WithMessageContaining("required");
    }

    [Fact]
    public void WithMessageContaining_NonMatching_ShouldThrow()
    {
        var act = () => Result.Failure<int>(Error.Validation("Name is required")).ShouldBeFailure()
            .WithMessageContaining("email");
        act.Should().Throw<ResultAssertionException>();
    }

    [Fact]
    public void WithError_CustomAssertion_ShouldWork()
    {
        int status = 0;
        Result.Failure<int>(Error.Unauthorized("ua")).ShouldBeFailure()
            .WithError(e => status = e.HttpStatus);
        status.Should().Be(401);
    }

    [Fact]
    public void WithError_FailingAssertion_ShouldThrow()
    {
        var act = () => Result.Failure<int>(Error.NotFound("nf")).ShouldBeFailure()
            .WithError(_ => throw new Exception("Custom fail"));
        act.Should().Throw<ResultAssertionException>()
            .WithMessage("*Error assertion failed*");
    }

    [Fact]
    public void WithInnerCode_Matching_ShouldNotThrow()
    {
        var inner = Error.Validation("inner");
        var outer = Error.Unexpected("outer").WithInner(inner);
        Result.Failure<int>(outer).ShouldBeFailure()
            .WithInnerCode("VALIDATION_ERROR");
    }

    [Fact]
    public void WithInnerCode_NoInner_ShouldThrow()
    {
        var act = () => Result.Failure<int>(Error.NotFound("nf")).ShouldBeFailure()
            .WithInnerCode("SOMETHING");
        act.Should().Throw<ResultAssertionException>()
            .WithMessage("*no inner error*");
    }

    [Fact]
    public void WithInnerCode_WrongCode_ShouldThrow()
    {
        var inner = Error.Validation("v");
        var outer = Error.Unexpected("u").WithInner(inner);
        var act = () => Result.Failure<int>(outer).ShouldBeFailure()
            .WithInnerCode("WRONG");
        act.Should().Throw<ResultAssertionException>()
            .WithMessage("*Expected inner error code*");
    }

    // ── Chaining ─────────────────────────────────────────────────────────────

    [Fact]
    public void ChainedAssertions_AllMatching_ShouldNotThrow()
    {
        Result.Failure<int>(Error.Validation("Bad input", "BAD_INPUT")).ShouldBeFailure()
            .WithCode("BAD_INPUT")
            .WithHttpStatus(422)
            .WithCategory(ErrorCategory.Validation)
            .WithMessage("Bad input")
            .WithMessageContaining("Bad");
    }
}

// ── ResultAssertionException Tests ───────────────────────────────────────────

public class ResultAssertionExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        var ex = new ResultAssertionException("Test message");
        ex.Message.Should().Be("Test message");
    }

    [Fact]
    public void Constructor_WithInner_ShouldSetInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new ResultAssertionException("outer", inner);
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void ShouldBeAssignableToException()
    {
        new ResultAssertionException("x").Should().BeAssignableTo<Exception>();
    }
}
