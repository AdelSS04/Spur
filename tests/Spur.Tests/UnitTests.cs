using Xunit;
using FluentAssertions;

namespace Spur.Tests;

public class UnitTests
{
    [Fact]
    public void Unit_ShouldHaveSingletonValue()
    {
        // Act
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;

        // Assert
        unit1.Should().Be(unit2);
    }

    [Fact]
    public void Unit_Equals_ShouldReturnTrue()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;

        // Act & Assert
        unit1.Equals(unit2).Should().BeTrue();
        (unit1 == unit2).Should().BeTrue();
        (unit1 != unit2).Should().BeFalse();
    }

    [Fact]
    public void Unit_GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;

        // Act & Assert
        unit1.GetHashCode().Should().Be(unit2.GetHashCode());
    }

    [Fact]
    public void Unit_CompareTo_ShouldReturnZero()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;

        // Act & Assert
        unit1.CompareTo(unit2).Should().Be(0);
    }

    [Fact]
    public void Unit_ToString_ShouldReturnUnit()
    {
        // Arrange
        var unit = Unit.Value;

        // Act
        var str = unit.ToString();

        // Assert
        str.Should().Be("()");
    }

    [Fact]
    public void Result_Success_Unit_ShouldWork()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }

    [Fact]
    public void Result_Failure_Unit_ShouldWork()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act
        var result = Result.Failure<Unit>(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}
