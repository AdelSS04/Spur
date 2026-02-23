---
sidebar_position: 2
---

# Quick Start

Build a small user API in about 10 minutes to see how Spur works in practice.

## What we're building

A minimal API with three endpoints:

- `GET /users/{id}` — get a user by ID
- `POST /users` — create a new user
- `PUT /users/{id}` — update a user

All errors are handled with `Result<T>` instead of exceptions.

## 1. Create the project

```bash
dotnet new webapi -n SpurDemo
cd SpurDemo
dotnet add package Spur
dotnet add package Spur.AspNetCore
```

## 2. Define the domain model

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public record CreateUserRequest(string Name, string Email);
public record UpdateUserRequest(string Name, string Email, bool IsActive);
```

## 3. Create a repository

```csharp
using Spur;

public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(int id);
    Task<Result<User>> CreateAsync(CreateUserRequest request);
    Task<Result<User>> UpdateAsync(int id, UpdateUserRequest request);
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new()
    {
        new User { Id = 1, Name = "Alice", Email = "alice@example.com" },
        new User { Id = 2, Name = "Bob", Email = "bob@example.com" }
    };

    private int _nextId = 3;

    public Task<Result<User>> GetByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);

        if (user is null)
            return Task.FromResult(
                Result.Failure<User>(Error.NotFound($"User {id} not found", "USER_NOT_FOUND")));

        if (!user.IsActive)
            return Task.FromResult(
                Result.Failure<User>(Error.Validation("User is inactive", "USER_INACTIVE")));

        return Task.FromResult(Result.Success(user));
    }

    public Task<Result<User>> CreateAsync(CreateUserRequest request)
    {
        if (_users.Any(u => u.Email == request.Email))
            return Task.FromResult(
                Result.Failure<User>(Error.Conflict("Email already exists", "EMAIL_EXISTS")));

        var user = new User
        {
            Id = _nextId++,
            Name = request.Name,
            Email = request.Email
        };

        _users.Add(user);
        return Task.FromResult(Result.Success(user));
    }

    public Task<Result<User>> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);

        if (user is null)
            return Task.FromResult(
                Result.Failure<User>(Error.NotFound($"User {id} not found", "USER_NOT_FOUND")));

        user.Name = request.Name;
        user.Email = request.Email;
        user.IsActive = request.IsActive;

        return Task.FromResult(Result.Success(user));
    }
}
```

Notice: every method returns `Result<User>`. No exceptions are thrown — failures are explicit values.

## 4. Wire up the API

```csharp
using Spur.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSpur();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

app.MapGet("/users/{id}", async (int id, IUserRepository repo) =>
{
    return await repo.GetByIdAsync(id)
        .ToHttpResult(); // 200 with user, or Problem Details on error
});

app.MapPost("/users", async (CreateUserRequest request, IUserRepository repo) =>
{
    return await repo.CreateAsync(request)
        .ToHttpResult(statusCode: 201); // 201 Created on success
});

app.MapPut("/users/{id}", async (int id, UpdateUserRequest request, IUserRepository repo) =>
{
    return await repo.UpdateAsync(id, request)
        .ToHttpResult();
});

app.Run();
```

## 5. Test it

```bash
dotnet run
```

**Success — GET /users/1** → 200 OK

```json
{
  "id": 1,
  "name": "Alice",
  "email": "alice@example.com",
  "isActive": true
}
```

**Not found — GET /users/999** → 404

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "User 999 not found",
  "errorCode": "USER_NOT_FOUND",
  "category": "NotFound"
}
```

**Conflict — POST /users** with a duplicate email → 409

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "Email already exists",
  "errorCode": "EMAIL_EXISTS",
  "category": "Conflict"
}
```

## What just happened?

- No `try`/`catch` blocks anywhere — all error handling is explicit through `Result<T>`.
- Every error automatically maps to the correct HTTP status code.
- All error responses follow RFC 7807 Problem Details.
- No exceptions are thrown on the hot path, so it's fast.

## Next steps

- [Your First Result](./your-first-result) — understand `Result<T>` in depth
- [Error types](../core-concepts/error-type) — learn about `Error` and `ErrorCategory`
- [Pipeline operators](../pipeline/then) — master `Then`, `Map`, `Validate`, and more
