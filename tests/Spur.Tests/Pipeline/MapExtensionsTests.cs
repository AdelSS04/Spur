using Xunit;
using FluentAssertions;
using Spur.Pipeline;
using Spur.Tests.Helpers;

namespace Spur.Tests.Pipeline;

public class MapExtensionsTests
{
    [Fact]
    public void Map_OnSuccess_ShouldTransformValue()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("42");
    }

    [Fact]
    public void Map_OnFailure_ShouldPropagateError()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.NotFound);

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(TestData.Errors.NotFound);
    }

    [Fact]
    public async Task MapAsync_WithAsyncFunc_OnSuccess_ShouldTransformValue()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("42");
    }

    [Fact]
    public async Task MapAsync_WithAsyncFunc_OnFailure_ShouldPropagateError()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.Validation);

        // Act
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(TestData.Errors.Validation);
    }

    [Fact]
    public async Task MapAsync_WithTaskResult_OnSuccess_ShouldTransformValue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(42));

        // Act
        var mapped = await resultTask.MapAsync(x => x * 2);

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(84);
    }

    [Fact]
    public async Task MapAsync_WithTaskResult_OnFailure_ShouldPropagateError()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Failure<int>(TestData.Errors.Conflict));

        // Act
        var mapped = await resultTask.MapAsync(x => x * 2);

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(TestData.Errors.Conflict);
    }

    [Fact]
    public async Task MapAsync_WithTaskResultAndAsyncFunc_ShouldWork()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(10));

        // Act
        var mapped = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x + 5;
        });

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(15);
    }

    [Fact]
    public void Map_ShouldChainMultipleMaps()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var mapped = result
            .Map(x => x * 2)
            .Map(x => x + 10)
            .Map(x => x.ToString());

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("20");
    }

    [Fact]
    public void Map_WithComplexType_ShouldWork()
    {
        // Arrange
        var result = Result.Success(TestData.SampleUser);

        // Act
        var mapped = result.Map(user => user.Email);

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Map_ToSameType_ShouldWork()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var mapped = result.Map(x => x + 1);

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(43);
    }
}
