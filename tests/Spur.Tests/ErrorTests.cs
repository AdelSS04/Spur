using FluentAssertions;
using Xunit;

namespace Spur.Tests;

public class ErrorTests
{
    // ── Factory Methods ──────────────────────────────────────────────────────

    [Fact]
    public void Validation_ShouldCreateWithCorrectDefaults()
    {
        var error = Error.Validation("Invalid input", "VALIDATION_ERROR");
        error.Code.Should().Be("VALIDATION_ERROR");
        error.Message.Should().Be("Invalid input");
        error.HttpStatus.Should().Be(422);
        error.Category.Should().Be(ErrorCategory.Validation);
        error.Extensions.Should().BeEmpty();
        error.Inner.Should().BeNull();
    }

    [Fact]
    public void Validation_WithDefaultCode()
    {
        var error = Error.Validation("Bad data");
        error.Code.Should().Be("VALIDATION_ERROR");
        error.HttpStatus.Should().Be(422);
    }

    [Fact]
    public void NotFound_ShouldCreateWithCorrectDefaults()
    {
        var error = Error.NotFound("Resource not found");
        error.Code.Should().Be("NOT_FOUND");
        error.Message.Should().Be("Resource not found");
        error.HttpStatus.Should().Be(404);
        error.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public void NotFound_WithCustomCode()
    {
        var error = Error.NotFound("User missing", "USER_NOT_FOUND");
        error.Code.Should().Be("USER_NOT_FOUND");
        error.HttpStatus.Should().Be(404);
    }

    [Fact]
    public void Unauthorized_ShouldCreateWithCorrectDefaults()
    {
        var error = Error.Unauthorized("Access denied");
        error.Code.Should().Be("UNAUTHORIZED");
        error.Message.Should().Be("Access denied");
        error.HttpStatus.Should().Be(401);
        error.Category.Should().Be(ErrorCategory.Unauthorized);
    }

    [Fact]
    public void Forbidden_ShouldCreateWithCorrectDefaults()
    {
        var error = Error.Forbidden("Not allowed");
        error.Code.Should().Be("FORBIDDEN");
        error.Message.Should().Be("Not allowed");
        error.HttpStatus.Should().Be(403);
        error.Category.Should().Be(ErrorCategory.Forbidden);
    }

    [Fact]
    public void Conflict_ShouldCreateWithCorrectDefaults()
    {
        var error = Error.Conflict("Already exists");
        error.Code.Should().Be("CONFLICT");
        error.Message.Should().Be("Already exists");
        error.HttpStatus.Should().Be(409);
        error.Category.Should().Be(ErrorCategory.Conflict);
    }

    [Fact]
    public void TooManyRequests_ShouldCreateWithCorrectDefaults()
    {
        var error = Error.TooManyRequests("Rate limit exceeded");
        error.Code.Should().Be("TOO_MANY_REQUESTS");
        error.Message.Should().Be("Rate limit exceeded");
        error.HttpStatus.Should().Be(429);
        error.Category.Should().Be(ErrorCategory.TooManyRequests);
    }

    [Fact]
    public void Unexpected_String_ShouldCreateWithCorrectDefaults()
    {
        var error = Error.Unexpected("Something went wrong");
        error.Code.Should().Be("UNEXPECTED_ERROR");
        error.Message.Should().Be("Something went wrong");
        error.HttpStatus.Should().Be(500);
        error.Category.Should().Be(ErrorCategory.Unexpected);
    }

    [Fact]
    public void Unexpected_FromException_ShouldCaptureMessage()
    {
        var ex = new InvalidOperationException("Boom!");
        var error = Error.Unexpected(ex);
        error.Code.Should().Be("UNEXPECTED_ERROR");
        error.Message.Should().Contain("Boom!");
        error.HttpStatus.Should().Be(500);
        error.Category.Should().Be(ErrorCategory.Unexpected);
    }

    [Fact]
    public void Unexpected_FromException_WithCustomCode()
    {
        var ex = new ArgumentException("Bad arg");
        var error = Error.Unexpected(ex, "ARG_ERROR");
        error.Code.Should().Be("ARG_ERROR");
        error.Message.Should().Contain("Bad arg");
    }

    [Fact]
    public void Custom_ShouldCreateWithCustomStatusCode()
    {
        var error = Error.Custom(418, "TEAPOT", "I'm a teapot", ErrorCategory.Custom);
        error.Code.Should().Be("TEAPOT");
        error.Message.Should().Be("I'm a teapot");
        error.HttpStatus.Should().Be(418);
        error.Category.Should().Be(ErrorCategory.Custom);
    }

    // ── Predefined Defaults ──────────────────────────────────────────────────

    [Fact]
    public void DefaultNotFound_ShouldHaveCorrectProperties()
    {
        Error.DefaultNotFound.Code.Should().Be("NOT_FOUND");
        Error.DefaultNotFound.HttpStatus.Should().Be(404);
        Error.DefaultNotFound.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public void DefaultValidation_ShouldHaveCorrectProperties()
    {
        Error.DefaultValidation.Code.Should().Be("VALIDATION_ERROR");
        Error.DefaultValidation.HttpStatus.Should().Be(422);
        Error.DefaultValidation.Category.Should().Be(ErrorCategory.Validation);
    }

    [Fact]
    public void DefaultUnauthorized_ShouldHaveCorrectProperties()
    {
        Error.DefaultUnauthorized.Code.Should().Be("UNAUTHORIZED");
        Error.DefaultUnauthorized.HttpStatus.Should().Be(401);
    }

    [Fact]
    public void DefaultForbidden_ShouldHaveCorrectProperties()
    {
        Error.DefaultForbidden.Code.Should().Be("FORBIDDEN");
        Error.DefaultForbidden.HttpStatus.Should().Be(403);
    }

    [Fact]
    public void DefaultConflict_ShouldHaveCorrectProperties()
    {
        Error.DefaultConflict.Code.Should().Be("CONFLICT");
        Error.DefaultConflict.HttpStatus.Should().Be(409);
    }

    [Fact]
    public void TooManyRequests_Factory_ShouldHaveCorrectProperties()
    {
        var error = Error.TooManyRequests("Rate limited");
        error.Code.Should().Be("TOO_MANY_REQUESTS");
        error.HttpStatus.Should().Be(429);
        error.Category.Should().Be(ErrorCategory.TooManyRequests);
    }

    [Fact]
    public void DefaultUnexpected_ShouldHaveCorrectProperties()
    {
        Error.DefaultUnexpected.Code.Should().Be("UNEXPECTED_ERROR");
        Error.DefaultUnexpected.HttpStatus.Should().Be(500);
    }

    // ── Modifiers ────────────────────────────────────────────────────────────

    [Fact]
    public void WithInner_ShouldSetInnerAndPreserveOuter()
    {
        var inner = Error.Validation("Inner");
        var outer = Error.Conflict("Outer", "OUTER");
        var result = outer.WithInner(inner);

        result.Code.Should().Be("OUTER");
        result.Message.Should().Be("Outer");
        result.HttpStatus.Should().Be(409);
        result.Inner.Should().NotBeNull();
        result.Inner!.Value.Code.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public void WithInner_ChainedDeep_ShouldSupportMultipleLevels()
    {
        var l3 = Error.Validation("L3");
        var l2 = Error.NotFound("L2").WithInner(l3);
        var l1 = Error.Unexpected("L1").WithInner(l2);

        l1.Inner!.Value.Code.Should().Be("NOT_FOUND");
        l1.Inner!.Value.Inner!.Value.Code.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public void WithMessage_ShouldOnlyChangeMessage()
    {
        var error = Error.NotFound("Original");
        var updated = error.WithMessage("New");

        updated.Message.Should().Be("New");
        updated.Code.Should().Be(error.Code);
        updated.HttpStatus.Should().Be(error.HttpStatus);
        updated.Category.Should().Be(error.Category);
    }

    [Fact]
    public void WithCode_ShouldOnlyChangeCode()
    {
        var error = Error.NotFound("MSG");
        var updated = error.WithCode("CUSTOM");

        updated.Code.Should().Be("CUSTOM");
        updated.Message.Should().Be("MSG");
        updated.HttpStatus.Should().Be(404);
    }

    [Fact]
    public void WithExtensions_AnonymousObject_ShouldConvertToDictionary()
    {
        var error = Error.Validation("Fail")
            .WithExtensions(new { field = "email", maxLen = 100 });

        error.Extensions["field"].Should().Be("email");
        error.Extensions["maxLen"].Should().Be(100);
    }

    [Fact]
    public void WithExtensions_ShouldNotMutateOriginal()
    {
        var error = Error.Validation("Fail");
        var updated = error.WithExtensions(new { key = "value" });

        error.Extensions.Should().BeEmpty();
        updated.Extensions.Should().ContainKey("key");
    }

    [Fact]
    public void ChainedModifiers_ShouldAllApply()
    {
        var error = Error.Validation("Original")
            .WithCode("CUSTOM_CODE")
            .WithMessage("Custom message")
            .WithExtensions(new { detail = "extra" })
            .WithInner(Error.NotFound("Inner"));

        error.Code.Should().Be("CUSTOM_CODE");
        error.Message.Should().Be("Custom message");
        error.Extensions.Should().ContainKey("detail");
        error.Inner.Should().NotBeNull();
        error.Inner!.Value.Category.Should().Be(ErrorCategory.NotFound);
    }

    // ── Equality ─────────────────────────────────────────────────────────────

    [Fact]
    public void Equality_SameProperties_ShouldBeEqual()
    {
        var a = Error.Validation("msg", "CODE");
        var b = Error.Validation("msg", "CODE");
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentCode_ShouldNotBeEqual()
    {
        var a = Error.Validation("msg", "A");
        var b = Error.Validation("msg", "B");
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_ErrorToResult_ShouldCreateFailure()
    {
        Result<int> result = Error.NotFound("Not found");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NOT_FOUND");
    }

    // ── Edge Cases ───────────────────────────────────────────────────────────

    [Fact]
    public void Extensions_EmptyByDefault()
    {
        Error.Validation("msg").Extensions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Inner_NullByDefault()
    {
        Error.Validation("msg").Inner.Should().BeNull();
    }

    [Theory]
    [InlineData(ErrorCategory.Validation)]
    [InlineData(ErrorCategory.NotFound)]
    [InlineData(ErrorCategory.Unauthorized)]
    [InlineData(ErrorCategory.Forbidden)]
    [InlineData(ErrorCategory.Conflict)]
    [InlineData(ErrorCategory.TooManyRequests)]
    [InlineData(ErrorCategory.Unexpected)]
    [InlineData(ErrorCategory.Custom)]
    public void AllCategories_ShouldBeDefined(ErrorCategory category)
    {
        Enum.IsDefined(category).Should().BeTrue();
    }
}
