using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Spur.MediatR.Tests;

// ── Test Request/Response Types ──────────────────────────────────────────────

public record GetUserQuery(int Id) : IRequest<Result<UserDto>>;
public record CreateUserCommand(string Name) : IRequest<Result<int>>;
public record DeleteUserCommand(int Id) : IRequest<Result<Unit>>;
public record UserDto(int Id, string Name);

// ── Test Handlers ────────────────────────────────────────────────────────────

public class GetUserQueryHandler : ResultHandler<GetUserQuery, UserDto>
{
    public override Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
            return Task.FromResult(Result.Failure<UserDto>(Error.Validation("Invalid ID")));

        return Task.FromResult(Result.Success(new UserDto(request.Id, "Test User")));
    }
}

public class DeleteUserCommandHandler : ResultHandler<DeleteUserCommand>
{
    public override Task<Result<Unit>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
            return Task.FromResult(Result.Failure<Unit>(Error.Validation("Invalid ID")));

        return Task.FromResult(Result.Success(Unit.Value));
    }
}

// ── ResultHandler Tests ──────────────────────────────────────────────────────

public class ResultHandlerTests
{
    [Fact]
    public async Task ResultHandler_Success_ShouldReturnSuccessResult()
    {
        var handler = new GetUserQueryHandler();
        var result = await handler.Handle(new GetUserQuery(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
        result.Value.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task ResultHandler_Failure_ShouldReturnFailureResult()
    {
        var handler = new GetUserQueryHandler();
        var result = await handler.Handle(new GetUserQuery(-1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.Validation);
    }

    [Fact]
    public async Task ResultHandler_Unit_Success_ShouldReturnUnit()
    {
        var handler = new DeleteUserCommandHandler();
        var result = await handler.Handle(new DeleteUserCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task ResultHandler_Unit_Failure_ShouldReturnFailure()
    {
        var handler = new DeleteUserCommandHandler();
        var result = await handler.Handle(new DeleteUserCommand(-1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

// ── ResultPipelineBehavior Tests ─────────────────────────────────────────────

public class ResultPipelineBehaviorTests
{
    private readonly Mock<ILogger<ResultPipelineBehavior<GetUserQuery, Result<UserDto>>>> _loggerMock = new();

    [Fact]
    public async Task Handle_Normal_ShouldPassThrough()
    {
        var behavior = new ResultPipelineBehavior<GetUserQuery, Result<UserDto>>(_loggerMock.Object);
        var request = new GetUserQuery(1);
        var expected = Result.Success(new UserDto(1, "Alice"));

        var result = await behavior.Handle(request, () => Task.FromResult(expected), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_SpurException_ShouldCatchAndReturnFailure()
    {
        var behavior = new ResultPipelineBehavior<GetUserQuery, Result<UserDto>>(_loggerMock.Object);
        var request = new GetUserQuery(1);
        var error = Error.NotFound("User not found");

        var result = await behavior.Handle(
            request,
            () => throw new SpurException(error),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_UnhandledException_ShouldCatchAndReturnUnexpectedError()
    {
        var behavior = new ResultPipelineBehavior<GetUserQuery, Result<UserDto>>(_loggerMock.Object);
        var request = new GetUserQuery(1);

        var result = await behavior.Handle(
            request,
            () => throw new InvalidOperationException("Something broke"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(ErrorCategory.Unexpected);
        result.Error.Code.Should().Be("UNHANDLED_PIPELINE_EXCEPTION");
    }

    [Fact]
    public async Task Handle_UnhandledException_ShouldIncludeRequestTypeInExtensions()
    {
        var behavior = new ResultPipelineBehavior<GetUserQuery, Result<UserDto>>(_loggerMock.Object);

        var result = await behavior.Handle(
            new GetUserQuery(1),
            () => throw new ArgumentException("bad arg"),
            CancellationToken.None);

        result.Error.Extensions.Should().ContainKey("requestType");
        result.Error.Extensions["requestType"].Should().Be("GetUserQuery");
    }

    [Fact]
    public async Task Handle_NullNext_ShouldThrowArgumentNullException()
    {
        var behavior = new ResultPipelineBehavior<GetUserQuery, Result<UserDto>>(_loggerMock.Object);

        var act = () => behavior.Handle(new GetUserQuery(1), null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_NullRequest_ShouldThrowArgumentNullException()
    {
        var behavior = new ResultPipelineBehavior<GetUserQuery, Result<UserDto>>(_loggerMock.Object);

        var act = () => behavior.Handle(
            null!,
            () => Task.FromResult(Result.Success(new UserDto(1, "x"))),
            CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_FailureResultFromHandler_ShouldPassThrough()
    {
        var behavior = new ResultPipelineBehavior<GetUserQuery, Result<UserDto>>(_loggerMock.Object);
        var error = Error.Forbidden("No access");

        var result = await behavior.Handle(
            new GetUserQuery(1),
            () => Task.FromResult(Result.Failure<UserDto>(error)),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FORBIDDEN");
    }
}
