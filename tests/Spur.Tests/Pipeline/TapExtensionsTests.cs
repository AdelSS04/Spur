using Xunit;
using FluentAssertions;
using Spur.Pipeline;
using Spur.Tests.Helpers;

namespace Spur.Tests.Pipeline;

public class TapExtensionsTests
{
    [Fact]
    public void Tap_OnSuccess_ShouldExecuteActionAndReturnOriginal()
    {
        // Arrange
        var result = Result.Success(42);
        var sideEffectExecuted = false;

        // Act
        var tapped = result.Tap(x =>
        {
            sideEffectExecuted = true;
            x.Should().Be(42);
        });

        // Assert
        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(42);
        sideEffectExecuted.Should().BeTrue();
    }

    [Fact]
    public void Tap_OnFailure_ShouldNotExecuteAction()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.NotFound);
        var sideEffectExecuted = false;

        // Act
        var tapped = result.Tap(x =>
        {
            sideEffectExecuted = true;
        });

        // Assert
        tapped.IsFailure.Should().BeTrue();
        tapped.Error.Should().Be(TestData.Errors.NotFound);
        sideEffectExecuted.Should().BeFalse();
    }

    [Fact]
    public void TapError_OnSuccess_ShouldNotExecuteAction()
    {
        // Arrange
        var result = Result.Success(42);
        var sideEffectExecuted = false;

        // Act
        var tapped = result.TapError(error =>
        {
            sideEffectExecuted = true;
        });

        // Assert
        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(42);
        sideEffectExecuted.Should().BeFalse();
    }

    [Fact]
    public void TapError_OnFailure_ShouldExecuteActionAndReturnOriginal()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.Validation);
        var sideEffectExecuted = false;

        // Act
        var tapped = result.TapError(error =>
        {
            sideEffectExecuted = true;
            error.Should().Be(TestData.Errors.Validation);
        });

        // Assert
        tapped.IsFailure.Should().BeTrue();
        tapped.Error.Should().Be(TestData.Errors.Validation);
        sideEffectExecuted.Should().BeTrue();
    }

    [Fact]
    public void TapBoth_OnSuccess_ShouldExecuteSuccessAction()
    {
        // Arrange
        var result = Result.Success(42);
        var successExecuted = false;
        var failureExecuted = false;

        // Act
        var tapped = result.TapBoth(
            onSuccess: x =>
            {
                successExecuted = true;
                x.Should().Be(42);
            },
            onError: error =>
            {
                failureExecuted = true;
            });

        // Assert
        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(42);
        successExecuted.Should().BeTrue();
        failureExecuted.Should().BeFalse();
    }

    [Fact]
    public void TapBoth_OnFailure_ShouldExecuteFailureAction()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.Conflict);
        var successExecuted = false;
        var failureExecuted = false;

        // Act
        var tapped = result.TapBoth(
            onSuccess: x =>
            {
                successExecuted = true;
            },
            onError: error =>
            {
                failureExecuted = true;
                error.Should().Be(TestData.Errors.Conflict);
            });

        // Assert
        tapped.IsFailure.Should().BeTrue();
        tapped.Error.Should().Be(TestData.Errors.Conflict);
        successExecuted.Should().BeFalse();
        failureExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_OnSuccess_ShouldExecuteAsyncAction()
    {
        // Arrange
        var result = Result.Success(42);
        var sideEffectExecuted = false;

        // Act
        var tapped = await result.TapAsync(async x =>
        {
            await Task.Delay(1);
            sideEffectExecuted = true;
            x.Should().Be(42);
        });

        // Assert
        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(42);
        sideEffectExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_WithTaskResult_ShouldWork()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(10));
        var sideEffectExecuted = false;

        // Act
        var tapped = await resultTask.TapAsync(x =>
        {
            sideEffectExecuted = true;
            x.Should().Be(10);
        });

        // Assert
        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(10);
        sideEffectExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_OnFailure_ShouldExecuteAsyncAction()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.Unauthorized);
        var sideEffectExecuted = false;

        // Act
        var tapped = await result.TapErrorAsync(async error =>
        {
            await Task.Delay(1);
            sideEffectExecuted = true;
            error.Should().Be(TestData.Errors.Unauthorized);
        });

        // Assert
        tapped.IsFailure.Should().BeTrue();
        tapped.Error.Should().Be(TestData.Errors.Unauthorized);
        sideEffectExecuted.Should().BeTrue();
    }

    [Fact]
    public void Tap_ForLogging_ShouldNotAffectPipeline()
    {
        // Arrange
        var result = Result.Success(TestData.SampleUser);
        var loggedValue = string.Empty;

        // Act
        var final = result
            .Tap(user => loggedValue = $"Processing user: {user.Name}")
            .Map(user => user.Email);

        // Assert
        final.IsSuccess.Should().BeTrue();
        final.Value.Should().Be("test@example.com");
        loggedValue.Should().Contain("Test User");
    }

    [Fact]
    public void TapError_ForErrorLogging_ShouldNotAffectPipeline()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.Unexpected);
        var loggedError = string.Empty;

        // Act
        var final = result
            .TapError(error => loggedError = $"Error: {error.Code}")
            .Map(x => x * 2);

        // Assert
        final.IsFailure.Should().BeTrue();
        final.Error.Should().Be(TestData.Errors.Unexpected);
        loggedError.Should().Contain("TEST_UNEXPECTED");
    }
}
