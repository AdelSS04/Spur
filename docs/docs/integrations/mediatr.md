---
sidebar_position: 4
---

# MediatR

The `Spur.MediatR` package integrates Spur with MediatR for CQRS architectures, providing base handler classes and automatic exception wrapping.

## Installation

```bash
dotnet add package Spur.MediatR
```

## ResultHandler base classes

### Query handler (returns a value)

```csharp
using Spur.MediatR;

public record GetUserQuery(int Id) : IRequest<Result<UserDto>>;

public class GetUserHandler : ResultHandler<GetUserQuery, UserDto>
{
    private readonly IUserRepository _repo;

    public GetUserHandler(IUserRepository repo) => _repo = repo;

    public override async Task<Result<UserDto>> Handle(
        GetUserQuery request, CancellationToken ct)
    {
        return await _repo.GetByIdAsync(request.Id)
            .Map(user => new UserDto(user.Id, user.Name, user.Email));
    }
}
```

### Command handler (no return value)

```csharp
public record DeleteUserCommand(int Id) : IRequest<Result<Unit>>;

public class DeleteUserHandler : ResultHandler<DeleteUserCommand>
{
    private readonly IUserRepository _repo;

    public DeleteUserHandler(IUserRepository repo) => _repo = repo;

    public override async Task<Result<Unit>> Handle(
        DeleteUserCommand request, CancellationToken ct)
    {
        return await _repo.DeleteAsync(request.Id);
    }
}
```

## ResultPipelineBehavior

Register the pipeline behavior to catch unhandled exceptions and wrap them as `Result.Failure`:

```csharp
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ResultPipelineBehavior<,>));
```

What it does:

- If the handler throws `SpurException`, it extracts the `Error` and returns `Result.Failure`.
- If the handler throws any other exception, it wraps it as `Error.Unexpected` and returns `Result.Failure`.
- It only activates when the response type is `Result<T>`.

## Wiring it up with ASP.NET Core

```csharp
using Spur.AspNetCore;

app.MapGet("/users/{id}", async (int id, IMediator mediator) =>
{
    return await mediator.Send(new GetUserQuery(id))
        .ToHttpResult();
});

app.MapDelete("/users/{id}", async (int id, IMediator mediator) =>
{
    return await mediator.Send(new DeleteUserCommand(id))
        .ToHttpResult(); // 204 No Content on success
});
```

## With FluentValidation

Combine with `Spur.FluentValidation` for validated commands:

```csharp
public class CreateUserHandler : ResultHandler<CreateUserCommand, UserDto>
{
    private readonly IValidator<CreateUserCommand> _validator;
    private readonly IUserRepository _repo;

    public override async Task<Result<UserDto>> Handle(
        CreateUserCommand request, CancellationToken ct)
    {
        return await Result.Start(request)
            .ValidateAsync(_validator, ct)
            .ThenAsync(r => _repo.CreateAsync(r))
            .Map(user => user.ToDto());
    }
}
```

## See also

- [ASP.NET Core](./aspnetcore) — HTTP response mapping
- [FluentValidation](./fluentvalidation) — input validation
- [Pipeline operators](../pipeline/then) — Result pipeline reference
