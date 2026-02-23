using Xunit;
using FluentAssertions;

namespace Spur.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act
        var result = Result.Failure<int>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_OnFailure_ShouldThrowException()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found"));

        // Act & Assert
        var action = () => result.Value;
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot access Value*");
    }

    [Fact]
    public void Error_OnSuccess_ShouldThrowException()
    {
        // Arrange
        var result = Result.Success(42);

        // Act & Assert
        var action = () => result.Error;
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot access Error*");
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        // Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act
        Result<int> result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Match_ShouldExecuteOnSuccess()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var output = result.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: error => $"Failure: {error.Code}");

        // Assert
        output.Should().Be("Success: 42");
    }

    [Fact]
    public void Match_ShouldExecuteOnFailure()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found"));

        // Act
        var output = result.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: error => $"Failure: {error.Code}");

        // Assert
        output.Should().Be("Failure: NOT_FOUND");
    }

    [Fact]
    public async Task MatchAsync_ShouldExecuteAsyncHandlers()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var output = await result.MatchAsync(
            onSuccess: async value => { await Task.Delay(1); return $"Success: {value}"; },
            onFailure: async error => { await Task.Delay(1); return $"Failure: {error.Code}"; });

        // Assert
        output.Should().Be("Success: 42");
    }

    [Fact]
    public void Unwrap_OnSuccess_ShouldReturnValue()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var value = result.Unwrap();

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void Unwrap_OnFailure_ShouldThrowSpurException()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found"));

        // Act & Assert
        var action = () => result.Unwrap();
        action.Should().Throw<SpurException>()
            .Which.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void UnwrapOr_OnSuccess_ShouldReturnValue()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var value = result.UnwrapOr(0);

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void UnwrapOr_OnFailure_ShouldReturnFallback()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found"));

        // Act
        var value = result.UnwrapOr(99);

        // Assert
        value.Should().Be(99);
    }

    [Fact]
    public void GetValueOrDefault_OnSuccess_ShouldReturnValue()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var value = result.GetValueOrDefault();

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ShouldReturnDefault()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found"));

        // Act
        var value = result.GetValueOrDefault();

        // Assert
        value.Should().Be(0);
    }

    [Fact]
    public void Try_ShouldCatchExceptions()
    {
        // Act
        var result = Result.Try(() =>
        {
            throw new InvalidOperationException("Test exception");
            return 42;
        });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.Unexpected);
        result.Error.Message.Should().Contain("Test exception");
    }

    [Fact]
    public async Task TryAsync_ShouldCatchExceptions()
    {
        // Act
        var result = await Result.TryAsync(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test exception");
            return 42;
        });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.Unexpected);
        result.Error.Message.Should().Contain("Test exception");
    }

    [Fact]
    public void Combine_AllSuccess_ShouldReturnSuccess()
    {
        // Arrange
        var result1 = Result.Success(1);
        var result2 = Result.Success(2);
        var result3 = Result.Success(3);

        // Act
        var combined = Result.Combine(result1, result2, result3);

        // Assert
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, 2, 3));
    }

    [Fact]
    public void Combine_AnyFailure_ShouldReturnFirstFailure()
    {
        // Arrange
        var result1 = Result.Success(1);
        var result2 = Result.Failure<int>(Error.NotFound("Error 2"));
        var result3 = Result.Failure<int>(Error.Validation("Error 3"));

        // Act
        var combined = Result.Combine(result1, result2, result3);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.Error.Code.Should().Be("NOT_FOUND");
    }
}
