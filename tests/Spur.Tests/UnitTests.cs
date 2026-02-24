using Xunit;
using FluentAssertions;

namespace Spur.Tests;

public class UnitTests
{
    [Fact]
    public void Value_ShouldBeSingleton()
    {
        Unit.Value.Should().Be(Unit.Value);
    }

    [Fact]
    public void Equals_SameValues_ShouldBeTrue()
    {
        var a = Unit.Value;
        var b = Unit.Value;
        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void Equals_BoxedObject_ShouldBeTrue()
    {
        object boxed = Unit.Value;
        Unit.Value.Equals(boxed).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        Unit.Value.GetHashCode().Should().Be(Unit.Value.GetHashCode());
    }

    [Fact]
    public void CompareTo_ShouldReturnZero()
    {
        Unit.Value.CompareTo(Unit.Value).Should().Be(0);
    }

    [Fact]
    public void ToString_ShouldReturnParens()
    {
        Unit.Value.ToString().Should().Be("()");
    }

    [Fact]
    public void Result_Success_Unit_ShouldWork()
    {
        var result = Result.Success();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }

    [Fact]
    public void Result_Failure_Unit_ShouldWork()
    {
        var error = Error.NotFound("Not found");
        var result = Result.Failure<Unit>(error);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Unit_ShouldBeEquatable()
    {
        IEquatable<Unit> equatable = Unit.Value;
        equatable.Equals(Unit.Value).Should().BeTrue();
    }

    [Fact]
    public void Unit_ShouldBeComparable()
    {
        IComparable<Unit> comparable = Unit.Value;
        comparable.CompareTo(Unit.Value).Should().Be(0);
    }
}
