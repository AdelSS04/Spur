using Xunit;
using FluentAssertions;
using Spur.Pipeline;
using Spur.Tests.Helpers;

namespace Spur.Tests.Pipeline;

public class RecoverExtensionsTests
{
    [Fact]
    public void Recover_OnSuccess_ShouldReturnOriginal()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var recovered = result.Recover(error => 99);

        // Assert
        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(42);
    }

    [Fact]
    public void Recover_OnFailure_ShouldRecoverWithFallback()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.NotFound);

        // Act
        var recovered = result.Recover(error => 99);

        // Assert
        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(99);
    }

    [Fact]
    public void Recover_OnFailure_ErrorParameterShouldBeCorrect()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.Validation);
        Error? capturedError = null;

        // Act
        var recovered = result.Recover(error =>
        {
            capturedError = error;
            return 100;
        });

        // Assert
        recovered.IsSuccess.Should().BeTrue();
        capturedError.Should().Be(TestData.Errors.Validation);
    }

    [Fact]
    public void RecoverIf_WithMatchingCategory_ShouldRecover()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.NotFound);

        // Act
        var recovered = result.RecoverIf(ErrorCategory.NotFound, error => 50);

        // Assert
        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(50);
    }

    [Fact]
    public void RecoverIf_WithNonMatchingCategory_ShouldPropagateError()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.NotFound);

        // Act
        var recovered = result.RecoverIf(ErrorCategory.Validation, error => 50);

        // Assert
        recovered.IsFailure.Should().BeTrue();
        recovered.Error.Should().Be(TestData.Errors.NotFound);
    }

    [Fact]
    public void RecoverIf_OnSuccess_ShouldReturnOriginal()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var recovered = result.RecoverIf(ErrorCategory.NotFound, error => 99);

        // Assert
        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(42);
    }

    [Fact]
    public void RecoverIfCode_WithMatchingCode_ShouldRecover()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found", "RESOURCE_NOT_FOUND"));

        // Act
        var recovered = result.RecoverIfCode("RESOURCE_NOT_FOUND", error => 75);

        // Assert
        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(75);
    }

    [Fact]
    public void RecoverIfCode_WithNonMatchingCode_ShouldPropagateError()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found", "RESOURCE_NOT_FOUND"));

        // Act
        var recovered = result.RecoverIfCode("DIFFERENT_CODE", error => 75);

        // Assert
        recovered.IsFailure.Should().BeTrue();
        recovered.Error.Code.Should().Be("RESOURCE_NOT_FOUND");
    }

    [Fact]
    public async Task RecoverAsync_OnFailure_ShouldRecoverWithAsyncFallback()
    {
        // Arrange
        var result = Result.Failure<int>(TestData.Errors.Conflict);

        // Act
        var recovered = await result.RecoverAsync(async error =>
        {
            await Task.Delay(1);
            return 200;
        });

        // Assert
        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(200);
    }

    [Fact]
    public async Task RecoverAsync_WithTaskResult_ShouldWork()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Failure<int>(TestData.Errors.Unauthorized));

        // Act
        var recovered = await resultTask.RecoverAsync(async error =>
        {
            await Task.Delay(1);
            return 150;
        });

        // Assert
        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(150);
    }

    [Fact]
    public void Recover_ChainedRecovery_ShouldUseFirstMatch()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound("Not found", "USER_NOT_FOUND"));

        // Act
        var recovered = result
            .RecoverIfCode("OTHER_ERROR", error => 1)
            .RecoverIf(ErrorCategory.NotFound, error => 2)
            .Recover(error => 3);

        // Assert
        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(2);
    }

    [Fact]
    public void Recover_ComplexScenario_ShouldHandleGracefully()
    {
        // Arrange
        var result = Result.Success(10)
            .Validate(x => x > 100, Error.Validation("Too small", "VALUE_TOO_SMALL"));

        // Act
        var recovered = result.RecoverIfCode("VALUE_TOO_SMALL", error => 100);

        // Assert
        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(100);
    }
}
