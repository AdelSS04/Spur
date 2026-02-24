using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spur.AspNetCore.Options;
using Xunit;
using FluentAssertions;

namespace Spur.AspNetCore.Tests;

public class ProblemDetailsMapperTests
{
    private readonly DefaultProblemDetailsMapper _mapper = new(new SpurOptions());

    // ── Basic Mapping ────────────────────────────────────────────────────────

    [Fact]
    public void ToProblemDetails_ShouldMapValidationError()
    {
        var error = Error.Validation("Name is required", "NAME_REQUIRED");
        var pd = _mapper.ToProblemDetails(error);

        pd.Status.Should().Be(422);
        pd.Detail.Should().Be("Name is required");
        pd.Type.Should().Contain("NAME_REQUIRED");
        pd.Extensions.Should().ContainKey("errorCode");
        pd.Extensions["errorCode"].Should().Be("NAME_REQUIRED");
    }

    [Fact]
    public void ToProblemDetails_ShouldMapNotFoundError()
    {
        var pd = _mapper.ToProblemDetails(Error.NotFound("User not found"));
        pd.Status.Should().Be(404);
        pd.Title.Should().Be("Not Found");
    }

    [Fact]
    public void ToProblemDetails_ShouldMapUnauthorizedError()
    {
        var pd = _mapper.ToProblemDetails(Error.Unauthorized("No token"));
        pd.Status.Should().Be(401);
        pd.Title.Should().Be("Unauthorized");
    }

    [Fact]
    public void ToProblemDetails_ShouldMapForbiddenError()
    {
        var pd = _mapper.ToProblemDetails(Error.Forbidden("No access"));
        pd.Status.Should().Be(403);
        pd.Title.Should().Be("Forbidden");
    }

    [Fact]
    public void ToProblemDetails_ShouldMapConflictError()
    {
        var pd = _mapper.ToProblemDetails(Error.Conflict("Duplicate"));
        pd.Status.Should().Be(409);
        pd.Title.Should().Be("Conflict");
    }

    [Fact]
    public void ToProblemDetails_ShouldMapTooManyRequestsError()
    {
        var pd = _mapper.ToProblemDetails(Error.TooManyRequests("Rate limit"));
        pd.Status.Should().Be(429);
        pd.Title.Should().Be("Too Many Requests");
    }

    [Fact]
    public void ToProblemDetails_ShouldMapUnexpectedError()
    {
        var pd = _mapper.ToProblemDetails(Error.Unexpected("Server error"));
        pd.Status.Should().Be(500);
        pd.Title.Should().Be("Internal Server Error");
    }

    // ── Options: Include/Exclude ─────────────────────────────────────────────

    [Fact]
    public void ToProblemDetails_IncludeErrorCategory_ShouldAddCategory()
    {
        var mapper = new DefaultProblemDetailsMapper(new SpurOptions { IncludeErrorCategory = true });
        var pd = mapper.ToProblemDetails(Error.Validation("v"));
        pd.Extensions.Should().ContainKey("category");
        pd.Extensions["category"].Should().Be("Validation");
    }

    [Fact]
    public void ToProblemDetails_ExcludeErrorCode_ShouldNotAddCode()
    {
        var mapper = new DefaultProblemDetailsMapper(new SpurOptions { IncludeErrorCode = false });
        var pd = mapper.ToProblemDetails(Error.Validation("v"));
        pd.Extensions.Should().NotContainKey("errorCode");
    }

    [Fact]
    public void ToProblemDetails_ExcludeCategory_ShouldNotAddCategory()
    {
        var mapper = new DefaultProblemDetailsMapper(new SpurOptions { IncludeErrorCategory = false });
        var pd = mapper.ToProblemDetails(Error.Validation("v"));
        pd.Extensions.Should().NotContainKey("category");
    }

    [Fact]
    public void ToProblemDetails_IncludeCustomExtensions_ShouldAddExtensions()
    {
        var error = Error.Validation("v").WithExtensions(new { field = "email" });
        var pd = _mapper.ToProblemDetails(error);
        pd.Extensions.Should().ContainKey("field");
        pd.Extensions["field"].Should().Be("email");
    }

    [Fact]
    public void ToProblemDetails_ExcludeCustomExtensions_ShouldNotAddExtensions()
    {
        var mapper = new DefaultProblemDetailsMapper(new SpurOptions { IncludeCustomExtensions = false });
        var error = Error.Validation("v").WithExtensions(new { field = "email" });
        var pd = mapper.ToProblemDetails(error);
        pd.Extensions.Should().NotContainKey("field");
    }

    [Fact]
    public void ToProblemDetails_IncludeInnerErrors_ShouldBuildInnerList()
    {
        var inner = Error.Validation("Inner issue");
        var outer = Error.Unexpected("Outer").WithInner(inner);
        var pd = _mapper.ToProblemDetails(outer);
        pd.Extensions.Should().ContainKey("innerErrors");
    }

    [Fact]
    public void ToProblemDetails_ExcludeInnerErrors_ShouldNotInclude()
    {
        var mapper = new DefaultProblemDetailsMapper(new SpurOptions { IncludeInnerErrors = false });
        var outer = Error.Unexpected("Outer").WithInner(Error.Validation("Inner"));
        var pd = mapper.ToProblemDetails(outer);
        pd.Extensions.Should().NotContainKey("innerErrors");
    }

    // ── Custom Status Mapper ─────────────────────────────────────────────────

    [Fact]
    public void ToProblemDetails_CustomStatusMapper_ShouldOverrideStatus()
    {
        var mapper = new DefaultProblemDetailsMapper(new SpurOptions
        {
            CustomStatusMapper = e => e.Category == ErrorCategory.Validation ? 400 : e.HttpStatus
        });
        var pd = mapper.ToProblemDetails(Error.Validation("v"));
        pd.Status.Should().Be(400);
        pd.Title.Should().Be("Bad Request");
    }

    // ── Custom Type BaseUri ──────────────────────────────────────────────────

    [Fact]
    public void ToProblemDetails_CustomBaseUri_ShouldUseCustomUri()
    {
        var mapper = new DefaultProblemDetailsMapper(new SpurOptions
        {
            ProblemDetailsTypeBaseUri = "https://myapi.com/errors/"
        });
        var pd = mapper.ToProblemDetails(Error.NotFound("nf"));
        pd.Type.Should().StartWith("https://myapi.com/errors/");
    }
}

public class ActionResultExtensionsTests
{
    private readonly IProblemDetailsMapper _mapper = new DefaultProblemDetailsMapper(new SpurOptions());

    // ── ToActionResult<T> ────────────────────────────────────────────────────

    [Fact]
    public void ToActionResult_OnSuccess_ShouldReturn200OkWithValue()
    {
        var result = Result.Success(42);
        var actionResult = result.ToActionResult(_mapper);

        actionResult.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)actionResult;
        ok.Value.Should().Be(42);
    }

    [Fact]
    public void ToActionResult_OnFailure_ShouldReturnProblemDetails()
    {
        var result = Result.Failure<int>(Error.NotFound("Not found"));
        var actionResult = result.ToActionResult(_mapper);

        actionResult.Should().BeOfType<ObjectResult>();
        var obj = (ObjectResult)actionResult;
        obj.StatusCode.Should().Be(404);
        obj.Value.Should().BeOfType<ProblemDetails>();
    }

    // ── ToActionResult(Unit) ─────────────────────────────────────────────────

    [Fact]
    public void ToActionResult_Unit_OnSuccess_ShouldReturn204NoContent()
    {
        var result = Result.Success(Unit.Value);
        var actionResult = result.ToActionResult(_mapper);

        actionResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_Unit_OnFailure_ShouldReturnProblemDetails()
    {
        var result = Result.Failure<Unit>(Error.Forbidden("No access"));
        var actionResult = result.ToActionResult(_mapper);

        var obj = (ObjectResult)actionResult;
        obj.StatusCode.Should().Be(403);
    }

    // ── ToActionResult with custom status code ──────────────────────────────

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(202)]
    [InlineData(204)]
    [InlineData(299)]
    public void ToActionResult_WithCustomSuccessStatus_ShouldReturnCorrectType(int status)
    {
        var result = Result.Success("value");
        var actionResult = result.ToActionResult(_mapper, status);

        if (status == 204)
            actionResult.Should().BeOfType<NoContentResult>();
        else
        {
            var obj = actionResult as ObjectResult;
            obj.Should().NotBeNull();
        }
    }

    // ── ToActionResultCreated ────────────────────────────────────────────────

    [Fact]
    public void ToActionResultCreated_OnSuccess_ShouldReturn201()
    {
        var result = Result.Success(new { Id = 1 });
        var actionResult = result.ToActionResultCreated(_mapper, "/api/items/1");

        actionResult.Should().BeOfType<CreatedResult>();
        var created = (CreatedResult)actionResult;
        created.Location.Should().Be("/api/items/1");
    }

    [Fact]
    public void ToActionResultCreated_OnFailure_ShouldReturnProblemDetails()
    {
        var result = Result.Failure<object>(Error.Validation("Invalid"));
        var actionResult = result.ToActionResultCreated(_mapper, "/api/items/1");

        actionResult.Should().BeOfType<ObjectResult>();
        ((ObjectResult)actionResult).StatusCode.Should().Be(422);
    }

    // ── Null mapper ──────────────────────────────────────────────────────────

    [Fact]
    public void ToActionResult_NullMapper_ShouldThrow()
    {
        var act = () => Result.Success(1).ToActionResult(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Async versions ───────────────────────────────────────────────────────

    [Fact]
    public async Task ToActionResultAsync_OnSuccess_ShouldReturn200()
    {
        var resultTask = Task.FromResult(Result.Success(42));
        var actionResult = await resultTask.ToActionResultAsync(_mapper);
        actionResult.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ToActionResultAsync_Unit_OnSuccess_ShouldReturn204()
    {
        var resultTask = Task.FromResult(Result.Success(Unit.Value));
        var actionResult = await resultTask.ToActionResultAsync(_mapper);
        actionResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ToActionResultAsync_WithStatus_ShouldWork()
    {
        var resultTask = Task.FromResult(Result.Success("data"));
        var actionResult = await resultTask.ToActionResultAsync(_mapper, 201);
        actionResult.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task ToActionResultCreatedAsync_ShouldWork()
    {
        var resultTask = Task.FromResult(Result.Success(new { Id = 1 }));
        var actionResult = await resultTask.ToActionResultCreatedAsync(_mapper, "/api/items/1");
        actionResult.Should().BeOfType<CreatedResult>();
    }
}

public class HttpResultExtensionsTests
{
    private readonly IProblemDetailsMapper _mapper = new DefaultProblemDetailsMapper(new SpurOptions());

    [Fact]
    public void ToHttpResult_OnSuccess_ShouldReturnOk()
    {
        var result = Result.Success(42).ToHttpResult(_mapper);
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToHttpResult_OnFailure_ShouldReturnProblem()
    {
        var result = Result.Failure<int>(Error.NotFound("nf")).ToHttpResult(_mapper);
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToHttpResult_Unit_OnSuccess_ShouldReturnNoContent()
    {
        var result = Result.Success(Unit.Value).ToHttpResult(_mapper);
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToHttpResult_Unit_OnFailure_ShouldReturnProblem()
    {
        var result = Result.Failure<Unit>(Error.Conflict("c")).ToHttpResult(_mapper);
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToHttpResult_WithCustomStatus_ShouldWork()
    {
        var result = Result.Success("data").ToHttpResult(_mapper, 201);
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToHttpResultCreated_OnSuccess_ShouldWork()
    {
        var result = Result.Success(new { Id = 1 }).ToHttpResultCreated(_mapper, "/api/items/1");
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToHttpResult_NullMapper_ShouldThrow()
    {
        var act = () => Result.Success(1).ToHttpResult(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToHttpResultCreated_NullLocation_ShouldThrow()
    {
        var act = () => Result.Success(1).ToHttpResultCreated(_mapper, null!);
        act.Should().Throw<ArgumentException>();
    }

    // ── Async ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ToHttpResultAsync_ShouldWork()
    {
        var result = await Task.FromResult(Result.Success(42)).ToHttpResultAsync(_mapper);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ToHttpResultAsync_Unit_ShouldWork()
    {
        var result = await Task.FromResult(Result.Success(Unit.Value)).ToHttpResultAsync(_mapper);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ToHttpResultAsync_WithStatus_ShouldWork()
    {
        var result = await Task.FromResult(Result.Success("x")).ToHttpResultAsync(_mapper, 202);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ToHttpResultCreatedAsync_ShouldWork()
    {
        var result = await Task.FromResult(Result.Success(1))
            .ToHttpResultCreatedAsync(_mapper, "/api/items/1");
        result.Should().NotBeNull();
    }
}

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSpur_ShouldRegisterDefaultMapper()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSpur();

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetService<IProblemDetailsMapper>();
        mapper.Should().NotBeNull();
        mapper.Should().BeOfType<DefaultProblemDetailsMapper>();
    }

    [Fact]
    public void AddSpur_WithOptions_ShouldApplyOptions()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSpur(opts => opts.ProblemDetailsTypeBaseUri = "https://custom.com/errors/");

        var provider = services.BuildServiceProvider();
        var options = provider.GetService<SpurOptions>();
        options.Should().NotBeNull();
        options!.ProblemDetailsTypeBaseUri.Should().Be("https://custom.com/errors/");
    }

    [Fact]
    public void AddSpur_WithCustomMapper_ShouldRegisterCustomMapper()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSpur<CustomTestMapper>();

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetService<IProblemDetailsMapper>();
        mapper.Should().BeOfType<CustomTestMapper>();
    }

    [Fact]
    public void AddSpur_CalledTwice_ShouldNotDuplicate()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSpur();
        services.AddSpur();

        var provider = services.BuildServiceProvider();
        var mappers = provider.GetServices<IProblemDetailsMapper>().ToList();
        mappers.Should().HaveCount(1);
    }

    private sealed class CustomTestMapper : IProblemDetailsMapper
    {
        public ProblemDetails ToProblemDetails(Error error) => new()
        {
            Status = error.HttpStatus,
            Detail = error.Message,
            Title = "Custom"
        };
    }
}

public class SpurOptionsTests
{
    [Fact]
    public void Defaults_ShouldHaveCorrectValues()
    {
        var opts = new SpurOptions();
        opts.ProblemDetailsTypeBaseUri.Should().Be("https://errors.example.com/");
        opts.IncludeErrorCode.Should().BeTrue();
        opts.IncludeErrorCategory.Should().BeTrue();
        opts.IncludeInnerErrors.Should().BeTrue();
        opts.IncludeCustomExtensions.Should().BeTrue();
        opts.CustomStatusMapper.Should().BeNull();
    }
}
