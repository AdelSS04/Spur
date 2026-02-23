using Xunit;
using FluentAssertions;
using Spur.Pipeline;
using Spur.Tests.Helpers;

namespace Spur.Tests.Pipeline;

public class MatchExtensionsTests
{
    [Fact]
    public void Match_OnSuccess_ShouldExecuteSuccessFunc()
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
    public void Match_OnFailure_ShouldExecuteFailureFunc()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.NotFound);

        // Act
        var output = result.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: error => $"Failure: {error.Code}");

        // Assert
        output.Should().Be("Failure: TEST_NOT_FOUND");
    }

    [Fact]
    public async Task MatchAsync_OnSuccess_ShouldExecuteSuccessFunc()
    {
        // Arrange
        var result = Result.Success(100);

        // Act
        var output = await result.MatchAsync(
            onSuccess: async value =>
            {
                await Task.Delay(1);
                return $"Async Success: {value}";
            },
            onFailure: async error =>
            {
                await Task.Delay(1);
                return $"Async Failure: {error.Code}";
            });

        // Assert
        output.Should().Be("Async Success: 100");
    }

    [Fact]
    public async Task MatchAsync_OnFailure_ShouldExecuteFailureFunc()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.Validation);

        // Act
        var output = await result.MatchAsync(
            onSuccess: async value =>
            {
                await Task.Delay(1);
                return $"Async Success: {value}";
            },
            onFailure: async error =>
            {
                await Task.Delay(1);
                return $"Async Failure: {error.Code}";
            });

        // Assert
        output.Should().Be("Async Failure: TEST_VALIDATION");
    }

    [Fact]
    public async Task MatchAsync_WithTaskResult_OnSuccess_ShouldWork()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(25));

        // Act
        var output = await resultTask.MatchAsync(
            onSuccess: value => $"Value: {value}",
            onFailure: error => $"Error: {error.Code}");

        // Assert
        output.Should().Be("Value: 25");
    }

    [Fact]
    public async Task MatchAsync_WithTaskResult_OnFailure_ShouldWork()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Failure<int>(TestData.Errors.Conflict));

        // Act
        var output = await resultTask.MatchAsync(
            onSuccess: value => $"Value: {value}",
            onFailure: error => $"Error: {error.Code}");

        // Assert
        output.Should().Be("Error: TEST_CONFLICT");
    }

    [Fact]
    public void Match_ComplexType_ShouldWork()
    {
        // Arrange
        var result = Result.Success(TestData.SampleUser);

        // Act
        var output = result.Match(
            onSuccess: user => $"{user.Name} ({user.Email})",
            onFailure: error => "No user");

        // Assert
        output.Should().Be("Test User (test@example.com)");
    }

    [Fact]
    public void Match_DifferentReturnTypes_ShouldWork()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var output = result.Match(
            onSuccess: value => value > 0,
            onFailure: error => false);

        // Assert
        output.Should().BeTrue();
    }

    [Fact]
    public void Match_InPipeline_ShouldTerminatePipeline()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var output = result
            .Map(x => x * 2)
            .Validate(x => x > 15, Error.Validation("Too small"))
            .Match(
                onSuccess: value => $"Final: {value}",
                onFailure: error => $"Error: {error.Message}");

        // Assert
        output.Should().Be("Final: 20");
    }

    [Fact]
    public void Match_WithErrorInPipeline_ShouldHandleGracefully()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var output = result
            .Map(x => x * 2)
            .Validate(x => x > 15, Error.Validation("Too small"))
            .Match(
                onSuccess: value => $"Final: {value}",
                onFailure: error => $"Error: {error.Message}");

        // Assert
        output.Should().Be("Error: Too small");
    }

    [Fact]
    public async Task MatchAsync_ComplexPipeline_ShouldWork()
    {
        // Arrange
        var result = Result.Success(TestData.SampleUser);

        // Act
        var output = await result
            .Map(user => user.Email)
            .MatchAsync(
                onSuccess: async email =>
                {
                    await Task.Delay(1);
                    return $"Email: {email}";
                },
                onFailure: async error =>
                {
                    await Task.Delay(1);
                    return "No email";
                });

        // Assert
        output.Should().Be("Email: test@example.com");
    }
}
