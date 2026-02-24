using FluentValidation;
using Spur.Pipeline;
using Xunit;
using FluentAssertions;

namespace Spur.FluentValidation.Tests;

// ── Test Models & Validators ─────────────────────────────────────────────────

public record CreateUserCommand(string Name, string Email, int Age);

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThan(0).LessThan(150);
    }
}

// ── ValidateToResult Tests ───────────────────────────────────────────────────

public class FluentValidationExtensionsTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void ValidateToResult_Valid_ShouldReturnSuccess()
    {
        var cmd = new CreateUserCommand("Alice", "alice@test.com", 25);
        var result = _validator.ValidateToResult(cmd);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(cmd);
    }

    [Fact]
    public void ValidateToResult_Invalid_ShouldReturnValidationError()
    {
        var cmd = new CreateUserCommand("", "not-an-email", -1);
        var result = _validator.ValidateToResult(cmd);

        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.Validation);
        result.Error.Code.Should().Be("VALIDATION_FAILED");
    }

    [Fact]
    public void ValidateToResult_Invalid_ShouldContainFieldErrors()
    {
        var cmd = new CreateUserCommand("", "bad", -1);
        var result = _validator.ValidateToResult(cmd);

        result.Error.Message.Should().Contain("Name");
        result.Error.Message.Should().Contain("Email");
        result.Error.Message.Should().Contain("Age");
    }

    [Fact]
    public void ValidateToResult_Invalid_ShouldHaveErrorsExtension()
    {
        var cmd = new CreateUserCommand("", "bad", -1);
        var result = _validator.ValidateToResult(cmd);

        result.Error.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public void ValidateToResult_SingleFieldInvalid_ShouldReportOnlyThatField()
    {
        var cmd = new CreateUserCommand("Alice", "not-email", 25);
        var result = _validator.ValidateToResult(cmd);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Email");
        result.Error.Message.Should().NotContain("Name:");
    }

    // ── ValidateToResultAsync ────────────────────────────────────────────────

    [Fact]
    public async Task ValidateToResultAsync_Valid_ShouldReturnSuccess()
    {
        var cmd = new CreateUserCommand("Bob", "bob@test.com", 30);
        var result = await _validator.ValidateToResultAsync(cmd);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(cmd);
    }

    [Fact]
    public async Task ValidateToResultAsync_Invalid_ShouldReturnValidationError()
    {
        var cmd = new CreateUserCommand("", "", 0);
        var result = await _validator.ValidateToResultAsync(cmd);

        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.Validation);
    }

    // ── Pipeline Extension: Validate ─────────────────────────────────────────

    [Fact]
    public void Validate_OnSuccess_Valid_ShouldReturnSuccess()
    {
        var cmd = new CreateUserCommand("Alice", "alice@test.com", 25);
        var result = Result.Success(cmd).Validate(_validator);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(cmd);
    }

    [Fact]
    public void Validate_OnSuccess_Invalid_ShouldReturnValidationError()
    {
        var cmd = new CreateUserCommand("", "", -1);
        var result = Result.Success(cmd).Validate(_validator);

        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.Validation);
    }

    [Fact]
    public void Validate_OnFailure_ShouldPropagateOriginalError()
    {
        var error = Error.NotFound("Not found");
        var result = Result.Failure<CreateUserCommand>(error).Validate(_validator);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    // ── Pipeline Extension: ValidateAsync ────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_Result_Valid_ShouldReturnSuccess()
    {
        var cmd = new CreateUserCommand("Alice", "alice@test.com", 25);
        var result = await Result.Success(cmd).ValidateAsync(_validator);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_Result_Invalid_ShouldReturnValidationError()
    {
        var cmd = new CreateUserCommand("", "", 0);
        var result = await Result.Success(cmd).ValidateAsync(_validator);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_Result_OnFailure_ShouldPropagateError()
    {
        var error = Error.Conflict("c");
        var result = await Result.Failure<CreateUserCommand>(error).ValidateAsync(_validator);

        result.Error.Should().Be(error);
    }

    // ── Pipeline Extension: ValidateAsync (Task<Result<T>>) ──────────────────

    [Fact]
    public async Task ValidateAsync_TaskResult_Valid_ShouldReturnSuccess()
    {
        var cmd = new CreateUserCommand("Alice", "alice@test.com", 25);
        var result = await Task.FromResult(Result.Success(cmd)).ValidateAsync(_validator);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_TaskResult_Invalid_ShouldReturnValidationError()
    {
        var cmd = new CreateUserCommand("", "", 0);
        var result = await Task.FromResult(Result.Success(cmd)).ValidateAsync(_validator);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VALIDATION_FAILED");
    }

    [Fact]
    public async Task ValidateAsync_TaskResult_OnFailure_ShouldPropagateError()
    {
        var error = Error.Unauthorized("no auth");
        var result = await Task.FromResult(Result.Failure<CreateUserCommand>(error))
            .ValidateAsync(_validator);

        result.Error.Should().Be(error);
    }

    // ── Null Checks ──────────────────────────────────────────────────────────

    [Fact]
    public void ValidateToResult_NullValidator_ShouldWork()
    {
        var act = () => ((IValidator<CreateUserCommand>)null!).ValidateToResult(
            new CreateUserCommand("A", "a@t.com", 1));
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateToResult_NullInstance_ShouldWork()
    {
        var act = () => _validator.ValidateToResult(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Complex Pipeline Scenario ────────────────────────────────────────────

    [Fact]
    public void FullPipeline_ValidateThenMap()
    {
        var cmd = new CreateUserCommand("Alice", "alice@test.com", 25);
        var result = Result.Success(cmd)
            .Validate(_validator)
            .Map(c => new { c.Name, c.Email });

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Alice");
    }

    [Fact]
    public void FullPipeline_InvalidValidation_ShouldShortCircuit()
    {
        bool mapCalled = false;
        var cmd = new CreateUserCommand("", "", 0);
        var result = Result.Success(cmd)
            .Validate(_validator)
            .Map(c =>
            {
                mapCalled = true;
                return c.Name;
            });

        mapCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }
}
