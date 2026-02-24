using FluentAssertions;
using Xunit;

namespace Spur.Tests;

public class SpurExceptionTests
{
    [Fact]
    public void Constructor_WithError_ShouldSetProperties()
    {
        var error = Error.NotFound("Not found");
        var ex = new SpurException(error);
        ex.Error.Should().Be(error);
        ex.Message.Should().Contain("Not found");
    }

    [Fact]
    public void Constructor_ShouldIncludeCodeInMessage()
    {
        var error = Error.Validation("Bad", "VAL_CODE");
        var ex = new SpurException(error);
        ex.Message.Should().Contain("VAL_CODE");
        ex.Message.Should().Contain("Bad");
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldSetInner()
    {
        var error = Error.Unexpected("Unexpected");
        var inner = new InvalidOperationException("inner-detail");
        var ex = new SpurException(error, inner);
        ex.Error.Should().Be(error);
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void Throw_ShouldBeCatchable()
    {
        var error = Error.Conflict("Conflict");
        Action act = () => throw new SpurException(error);
        act.Should().Throw<SpurException>()
            .Which.Error.Code.Should().Be("CONFLICT");
    }

    [Fact]
    public void ShouldBeAssignableToException()
    {
        var ex = new SpurException(Error.NotFound("x"));
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void AllErrorCategories_ShouldWorkWithException()
    {
        var errors = new[]
        {
            Error.Validation("v"), Error.NotFound("n"),
            Error.Unauthorized("u"), Error.Forbidden("f"),
            Error.Conflict("c"), Error.TooManyRequests("t"),
            Error.Unexpected("x")
        };

        foreach (var error in errors)
        {
            var ex = new SpurException(error);
            ex.Error.Category.Should().NotBe(ErrorCategory.Custom);
        }
    }
}
