using Xunit;
using FluentAssertions;
using Spur.Pipeline;
using Spur.Tests.Helpers;

namespace Spur.Tests.Pipeline;

public class ValidateExtensionsTests
{
    [Fact]
    public void Validate_WithPredicateTrue_ShouldReturnSuccess()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var validated = result.Validate(x => x > 0, Error.Validation("Must be positive"));

        // Assert
        validated.IsSuccess.Should().BeTrue();
        validated.Value.Should().Be(42);
    }

    [Fact]
    public void Validate_WithPredicateFalse_ShouldReturnFailure()
    {
        // Arrange
        var result = Result.Success(42);
        var error = Error.Validation("Must be less than 10");

        // Act
        var validated = result.Validate(x => x < 10, error);

        // Assert
        validated.IsFailure.Should().BeTrue();
        validated.Error.Should().Be(error);
    }

    [Fact]
    public void Validate_OnFailure_ShouldPropagateError()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.NotFound);

        // Act
        var validated = result.Validate(x => x > 0, Error.Validation("Should not run"));

        // Assert
        validated.IsFailure.Should().BeTrue();
        validated.Error.Should().Be(TestData.Errors.NotFound);
    }

    [Fact]
    public void Validate_WithErrorFactory_ShouldCallFactoryOnFailure()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var validated = result.Validate(
            x => x < 10,
            x => Error.Validation($"Value {x} is too large", "VALUE_TOO_LARGE"));

        // Assert
        validated.IsFailure.Should().BeTrue();
        validated.Error.Code.Should().Be("VALUE_TOO_LARGE");
        validated.Error.Message.Should().Contain("42");
    }

    [Fact]
    public void Validate_WithErrorFactory_OnSuccess_ShouldNotCallFactory()
    {
        // Arrange
        var result = Result.Success(5);
        var factoryCalled = false;

        // Act
        var validated = result.Validate(
            x => x < 10,
            x =>
            {
                factoryCalled = true;
                return Error.Validation("Should not be called");
            });

        // Assert
        validated.IsSuccess.Should().BeTrue();
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WithAsyncPredicate_OnSuccess_ShouldWork()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var validated = await result.ValidateAsync(
            async x =>
            {
                await Task.Delay(1);
                return x > 0;
            },
            Error.Validation("Must be positive"));

        // Assert
        validated.IsSuccess.Should().BeTrue();
        validated.Value.Should().Be(42);
    }

    [Fact]
    public async Task ValidateAsync_WithAsyncPredicate_OnFailure_ShouldReturnError()
    {
        // Arrange
        var result = Result.Success(42);
        var error = Error.Validation("Too large");

        // Act
        var validated = await result.ValidateAsync(
            async x =>
            {
                await Task.Delay(1);
                return x < 10;
            },
            error);

        // Assert
        validated.IsFailure.Should().BeTrue();
        validated.Error.Should().Be(error);
    }

    [Fact]
    public async Task ValidateAsync_WithTaskResult_ShouldWork()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(15));

        // Act
        var validated = await resultTask.ValidateAsync(
            x => x > 10,
            Error.Validation("Must be greater than 10"));

        // Assert
        validated.IsSuccess.Should().BeTrue();
        validated.Value.Should().Be(15);
    }

    [Fact]
    public async Task ValidateAsync_WithAsyncPredicate_AndErrorFactory_ShouldWork()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var validated = await result.ValidateAsync(
            async x =>
            {
                await Task.Delay(1);
                return x < 10;
            },
            x => Error.Validation($"Value {x} is invalid", "INVALID_VALUE"));

        // Assert
        validated.IsFailure.Should().BeTrue();
        validated.Error.Code.Should().Be("INVALID_VALUE");
    }

    [Fact]
    public void Validate_ChainedValidations_ShouldStopAtFirstFailure()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var validated = result
            .Validate(x => x > 0, Error.Validation("Must be positive"))
            .Validate(x => x < 10, Error.Validation("Must be less than 10", "TOO_LARGE"))
            .Validate(x => x % 2 == 0, Error.Validation("Must be even"));

        // Assert
        validated.IsFailure.Should().BeTrue();
        validated.Error.Code.Should().Be("TOO_LARGE");
    }

    [Fact]
    public void Validate_AllPass_ShouldReturnOriginalValue()
    {
        // Arrange
        var result = Result.Success(8);

        // Act
        var validated = result
            .Validate(x => x > 0, Error.Validation("Must be positive"))
            .Validate(x => x < 10, Error.Validation("Must be less than 10"))
            .Validate(x => x % 2 == 0, Error.Validation("Must be even"));

        // Assert
        validated.IsSuccess.Should().BeTrue();
        validated.Value.Should().Be(8);
    }
}
