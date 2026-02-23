---
sidebar_position: 1
---

# ASP.NET Core

The `Spur.AspNetCore` package converts `Result<T>` values into HTTP responses automatically — `200 OK` for success, RFC 7807 Problem Details for errors.

## Installation

```bash
dotnet add package Spur.AspNetCore
```

## Setup

Register Spur services in `Program.cs`:

```csharp
using Spur.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSpur();

var app = builder.Build();
```

### With custom options

```csharp
builder.Services.AddSpur(options =>
{
    options.ProblemDetailsTypeBaseUri = "https://api.myapp.com/errors/";
    options.IncludeErrorCode = true;
    options.IncludeErrorCategory = true;
    options.IncludeInnerErrors = false;       // hide inner errors in production
    options.IncludeCustomExtensions = true;
});
```

### SpurOptions reference

| Property | Default | Description |
|---|---|---|
| `ProblemDetailsTypeBaseUri` | `"https://errors.example.com/"` | Base URI for the `type` field in Problem Details |
| `IncludeErrorCode` | `true` | Include `errorCode` in the response |
| `IncludeErrorCategory` | `true` | Include `category` in the response |
| `IncludeInnerErrors` | `true` | Include inner error chain |
| `IncludeCustomExtensions` | `true` | Include custom extensions |
| `CustomStatusMapper` | `null` | Override HTTP status code mapping |

## Minimal APIs

### ToHttpResult

Convert a `Result<T>` to an `IResult`:

```csharp
app.MapGet("/users/{id}", async (int id, IUserRepository repo) =>
{
    return await repo.GetByIdAsync(id)
        .ToHttpResult();
});
```

- Success → `200 OK` with the value as JSON
- Failure → Problem Details with the error's HTTP status code

### Custom success status code

```csharp
app.MapPost("/users", async (CreateUserRequest req, IUserRepository repo) =>
{
    return await repo.CreateAsync(req)
        .ToHttpResult(statusCode: 201); // 201 Created
});
```

### Created with location

```csharp
app.MapPost("/users", async (CreateUserRequest req, IUserRepository repo) =>
{
    return await repo.CreateAsync(req)
        .ToHttpResultCreated($"/users/{req.Email}");
});
```

### Void operations (Result&lt;Unit&gt;)

When the Result has no meaningful value, a success returns `204 No Content`:

```csharp
app.MapDelete("/users/{id}", async (int id, IUserRepository repo) =>
{
    return await repo.DeleteAsync(id)
        .ToHttpResult(); // 204 No Content on success
});
```

## MVC Controllers

Use `ToActionResult` in controllers:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;
    private readonly IProblemDetailsMapper _mapper;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        return await _repo.GetByIdAsync(id)
            .ToActionResult(_mapper);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        return await _repo.CreateAsync(request)
            .ToActionResult(_mapper, successStatusCode: 201);
    }
}
```

## SpurMiddleware

Optionally add middleware that catches unhandled exceptions and converts them to Problem Details:

```csharp
app.UseMiddleware<SpurMiddleware>();
```

This catches:
- `SpurException` → maps the inner `Error` to Problem Details
- Any other `Exception` → wraps as `Error.Unexpected` → `500` Problem Details

## Problem Details response format

Every error produces an RFC 7807-compliant response:

```json
{
  "type": "https://errors.example.com/USER_NOT_FOUND",
  "title": "Not Found",
  "status": 404,
  "detail": "User 123 not found",
  "errorCode": "USER_NOT_FOUND",
  "category": "NotFound"
}
```

## Custom ProblemDetailsMapper

Implement `IProblemDetailsMapper` for full control:

```csharp
public class MyMapper : IProblemDetailsMapper
{
    public ProblemDetails ToProblemDetails(Error error)
    {
        return new ProblemDetails
        {
            Type = $"https://api.myapp.com/errors/{error.Code}",
            Title = error.Category.ToString(),
            Status = error.HttpStatus,
            Detail = error.Message
        };
    }
}

// Register
builder.Services.AddSpur<MyMapper>();
```

## See also

- [Error type](../core-concepts/error-type) — how errors carry HTTP status codes
- [Quick Start](../getting-started/quick-start) — build an API from scratch
