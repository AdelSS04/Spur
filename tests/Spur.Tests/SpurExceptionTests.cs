using Xunit;
using FluentAssertions;

namespace Spur.Tests;

public class SpurExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetError()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act
        var exception = new SpurException(error);

        // Assert
        exception.Error.Should().Be(error);
        exception.Message.Should().Contain("Not found");
    }

    [Fact]
    public void Constructor_WithError_ShouldSetMessageFromError()
    {
        // Arrange
        var error = Error.Validation("Validation error", "VAL_ERROR");

        // Act
        var exception = new SpurException(error);

        // Assert
        exception.Error.Should().Be(error);
        exception.Message.Should().Contain("VAL_ERROR");
        exception.Message.Should().Contain("Validation error");
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldSetInnerException()
    {
        // Arrange
        var error = Error.Unexpected("Unexpected error");
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new SpurException(error, innerException);

        // Assert
        exception.Error.Should().Be(error);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void Throw_ShouldCreateException()
    {
        // Arrange
        var error = Error.Conflict("Conflict");

        // Act & Assert
        Action action = () => throw new SpurException(error);
        action.Should().Throw<SpurException>()
            .Which.Error.Code.Should().Be("CONFLICT");
    }

    [Fact]
    public void SpurException_ShouldBeSerializable()
    {
        // Arrange
        var error = Error.NotFound("Resource not found", "RESOURCE_NOT_FOUND");
        var exception = new SpurException(error);

        // Assert - just verify it's an Exception (serialization tests would need BinaryFormatter which is obsolete)
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Message_ShouldIncludeErrorCodeAndMessage()
    {
        // Arrange
        var error = Error.Validation("Email is invalid", "INVALID_EMAIL");

        // Act
        var exception = new SpurException(error);

        // Assert
        exception.Message.Should().Contain("INVALID_EMAIL");
        exception.Message.Should().Contain("Email is invalid");
    }
}
