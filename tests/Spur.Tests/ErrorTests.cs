using Xunit;
using FluentAssertions;

namespace Spur.Tests;

public class ErrorTests
{
    [Fact]
    public void Validation_ShouldCreateErrorWithCorrectProperties()
    {
        // Act
        var error = Error.Validation("Invalid input", "VALIDATION_ERROR");

        // Assert
        error.Code.Should().Be("VALIDATION_ERROR");
        error.Message.Should().Be("Invalid input");
        error.HttpStatus.Should().Be(422);
        error.Category.Should().Be(ErrorCategory.Validation);
    }

    [Fact]
    public void NotFound_ShouldCreateErrorWithCorrectProperties()
    {
        // Act
        var error = Error.NotFound("Resource not found");

        // Assert
        error.Code.Should().Be("NOT_FOUND");
        error.Message.Should().Be("Resource not found");
        error.HttpStatus.Should().Be(404);
        error.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public void Unauthorized_ShouldCreateErrorWithCorrectProperties()
    {
        // Act
        var error = Error.Unauthorized("Access denied");

        // Assert
        error.Code.Should().Be("UNAUTHORIZED");
        error.Message.Should().Be("Access denied");
        error.HttpStatus.Should().Be(401);
        error.Category.Should().Be(ErrorCategory.Unauthorized);
    }

    [Fact]
    public void Conflict_ShouldCreateErrorWithCorrectProperties()
    {
        // Act
        var error = Error.Conflict("Resource already exists");

        // Assert
        error.Code.Should().Be("CONFLICT");
        error.Message.Should().Be("Resource already exists");
        error.HttpStatus.Should().Be(409);
        error.Category.Should().Be(ErrorCategory.Conflict);
    }

    [Fact]
    public void Unexpected_ShouldCreateErrorWithCorrectProperties()
    {
        // Act
        var error = Error.Unexpected("Something went wrong");

        // Assert
        error.Code.Should().Be("UNEXPECTED_ERROR");
        error.Message.Should().Be("Something went wrong");
        error.HttpStatus.Should().Be(500);
        error.Category.Should().Be(ErrorCategory.Unexpected);
    }

    [Fact]
    public void WithInner_ShouldSetInnerError()
    {
        // Arrange
        var innerError = Error.Validation("Inner validation error");
        var outerError = Error.Unexpected("Outer error");

        // Act
        var errorWithInner = outerError.WithInner(innerError);

        // Assert
        errorWithInner.Inner.Should().NotBeNull();
        errorWithInner.Inner!.Value.Code.Should().Be("VALIDATION_ERROR");
        errorWithInner.Inner!.Value.Message.Should().Be("Inner validation error");
    }

    [Fact]
    public void WithMessage_ShouldUpdateMessage()
    {
        // Arrange
        var error = Error.NotFound("Original message");

        // Act
        var updated = error.WithMessage("Updated message");

        // Assert
        updated.Message.Should().Be("Updated message");
        updated.Code.Should().Be(error.Code);
        updated.HttpStatus.Should().Be(error.HttpStatus);
    }

    [Fact]
    public void WithCode_ShouldUpdateCode()
    {
        // Arrange
        var error = Error.NotFound("Message");

        // Act
        var updated = error.WithCode("CUSTOM_NOT_FOUND");

        // Assert
        updated.Code.Should().Be("CUSTOM_NOT_FOUND");
        updated.Message.Should().Be(error.Message);
    }

    [Fact]
    public void WithExtensions_ShouldAddExtensionData()
    {
        // Arrange
        var error = Error.Validation("Validation failed");

        // Act
        var updated = error.WithExtensions(new { fieldName = "email", reason = "invalid format" });

        // Assert
        updated.Extensions.Should().ContainKey("fieldName");
        updated.Extensions.Should().ContainKey("reason");
        updated.Extensions["fieldName"].Should().Be("email");
        updated.Extensions["reason"].Should().Be("invalid format");
    }

    [Fact]
    public void Custom_ShouldCreateErrorWithCustomStatusCode()
    {
        // Act
        var error = Error.Custom(418, "CUSTOM_ERROR", "Custom error", ErrorCategory.Custom);

        // Assert
        error.Code.Should().Be("CUSTOM_ERROR");
        error.Message.Should().Be("Custom error");
        error.HttpStatus.Should().Be(418);
        error.Category.Should().Be(ErrorCategory.Custom);
    }
}
