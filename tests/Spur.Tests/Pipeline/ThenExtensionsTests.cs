using Xunit;
using FluentAssertions;
using Spur.Pipeline;

namespace Spur.Tests.Pipeline;

public class ThenExtensionsTests
{
    [Fact]
    public void Then_OnSuccess_ShouldExecuteFunction()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var newResult = result.Then(x => Result.Success(x * 2));

        // Assert
        newResult.IsSuccess.Should().BeTrue();
        newResult.Value.Should().Be(20);
    }

    [Fact]
    public void Then_OnFailure_ShouldNotExecuteFunction()
    {
        // Arrange
        var error = Error.NotFound("Not found");
        var result = Result.Failure<int>(error);
        var executed = false;

        // Act
        var newResult = result.Then(x =>
        {
            executed = true;
            return Result.Success(x * 2);
        });

        // Assert
        executed.Should().BeFalse();
        newResult.IsFailure.Should().BeTrue();
        newResult.Error.Should().Be(error);
    }

    [Fact]
    public void Then_FunctionReturnsFailure_ShouldPropagateFailure()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var newResult = result.Then(x => Result.Failure<int>(Error.Validation("Invalid")));

        // Assert
        newResult.IsFailure.Should().BeTrue();
        newResult.Error.Code.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task ThenAsync_OnSuccess_ShouldExecuteAsyncFunction()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var newResult = await result.ThenAsync(async x =>
        {
            await Task.Delay(1);
            return Result.Success(x * 2);
        });

        // Assert
        newResult.IsSuccess.Should().BeTrue();
        newResult.Value.Should().Be(20);
    }

    [Fact]
    public async Task ThenAsync_WithTaskResult_ShouldChainCorrectly()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(10));

        // Act
        var newResult = await resultTask.ThenAsync(x => Result.Success(x * 2));

        // Assert
        newResult.IsSuccess.Should().BeTrue();
        newResult.Value.Should().Be(20);
    }

    [Fact]
    public void Then_WithNullChecking_ShouldReturnNotFoundOnNull()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var newResult = result.Then<int, string>(x => (string?)null, Error.DefaultNotFound);

        // Assert
        newResult.IsFailure.Should().BeTrue();
        newResult.Error.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public void Then_WithNullChecking_NonNull_ShouldReturnSuccess()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var newResult = result.Then<int, string>(x => "result");

        // Assert
        newResult.IsSuccess.Should().BeTrue();
        newResult.Value.Should().Be("result");
    }
}
