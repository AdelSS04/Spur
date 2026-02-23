# Spur

**Spur-Oriented Programming for .NET**

HTTP-first, fluent, AOT-ready error handling for ASP.NET Core with zero core dependencies.

[![NuGet](https://img.shields.io/nuget/v/Spur.svg)](https://nuget.org/packages/Spur)
[![Build](https://github.com/AdelSS04/Spur/workflows/CI/badge.svg)](https://github.com/AdelSS04/Spur/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Why Spur?

Stop throwing exceptions for business logic failures. Stop writing the same error handling middleware in every project. Start using Spur-Oriented Programming.

```csharp
// ❌ OLD WAY: Exceptions as control flow
public async Task<UserDto> GetUser(int id)
{
    var user = await _repo.FindAsync(id);
    if (user == null) throw new NotFoundException("User not found");  // 10-1000x slower
    if (!user.IsActive) throw new ValidationException("User inactive");
    return _mapper.Map<UserDto>(user);
}

// ✅ NEW WAY: Explicit, type-safe, fast
public async Task<Result<UserDto>> GetUser(int id)
{
    return await Result.Start(id)
        .ThenAsync(async id => await _repo.FindAsync(id), Error.NotFound("User not found"))
        .Validate(user => user.IsActive, Error.Validation("User inactive"))
        .Map(user => _mapper.Map<UserDto>(user));
}
```

## Features

- 🚀 **Zero allocations** on success path — `readonly struct Result<T>`
- 🌐 **HTTP-first** — Every `Error` carries an HTTP status code
- 🔗 **Fluent pipeline** — `Then → Map → Validate → Tap → Recover → Match`
- ⚡ **10-100× faster** than exceptions for error paths
- 🎯 **Type-safe** — Compiler-enforced error handling
- 📦 **Zero core dependencies** — Spur has no external dependencies
- 🔍 **Roslyn analyzers** — Catch `Result` misuse at compile time
- 🧪 **Test-friendly** — Built-in fluent assertions
- 🏗️ **Native AOT compatible** — via source generators

## Installation

### Core Library (Required)
```bash
dotnet add package Spur
```

### Choose Your Integrations

```bash
# For ASP.NET Core Minimal APIs or MVC
dotnet add package Spur.AspNetCore

# For Entity Framework Core
dotnet add package Spur.EntityFrameworkCore

# For FluentValidation
dotnet add package Spur.FluentValidation

# For MediatR (CQRS)
dotnet add package Spur.MediatR

# For unit testing
dotnet add package Spur.Testing

# For Native AOT (optional, enhances AspNetCore)
dotnet add package Spur.Generators

# For compile-time safety checks
dotnet add package Spur.Analyzers
```

## Package Guide

| Package | Install When | Dependencies |
|---------|-------------|--------------|
| **Spur** | Always (core library) | None ✅ |
| **Spur.AspNetCore** | Using ASP.NET Core APIs | Microsoft.AspNetCore.App |
| **Spur.EntityFrameworkCore** | Using EF Core queries | Microsoft.EntityFrameworkCore |
| **Spur.FluentValidation** | Using FluentValidation | FluentValidation |
| **Spur.MediatR** | Using MediatR/CQRS | MediatR |
| **Spur.Testing** | Writing unit tests | None ✅ |
| **Spur.Generators** | Deploying with Native AOT | Roslyn (build-time) |
| **Spur.Analyzers** | Want compile-time checks | Roslyn (build-time) |

## Quick Start

### 1. Basic Error Handling

```csharp
using Spur;

public Result<int> Divide(int numerator, int denominator)
{
    if (denominator == 0)
        return Error.Validation("Cannot divide by zero", "DIVISION_BY_ZERO");

    return Result.Success(numerator / denominator);
}

// Use it
var result = Divide(10, 2);
if (result.IsSuccess)
    Console.WriteLine($"Result: {result.Value}");
else
    Console.WriteLine($"Error: {result.Error.Message}");
```

### 2. ASP.NET Core Minimal API

```csharp
using Spur.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSpur(); // Register Problem Details mapper
var app = builder.Build();

app.MapGet("/users/{id}", async (int id, IUserRepository repo) =>
{
    return await repo.GetUserAsync(id)
        .ToHttpResult();  // Returns 200 OK or RFC 7807 Problem Details
});

// POST endpoint with validation
app.MapPost("/users", async (CreateUserRequest request,
    IValidator<CreateUserRequest> validator,
    IUserRepository repo) =>
{
    return await Result.Start(request)
        .ValidateAsync(validator)
        .ThenAsync(async req => await repo.CreateAsync(req))
        .ToHttpResult(mapper, successStatusCode: 201);
});
```

**Output examples:**

Success (200 OK):
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com"
}
```

Failure (404 Not Found):
```json
{
  "type": "https://api.example.com/errors/USER_NOT_FOUND",
  "title": "Not Found",
  "status": 404,
  "detail": "User with ID 999 not found",
  "errorCode": "USER_NOT_FOUND",
  "category": "NotFound"
}
```

### 3. Entity Framework Core Integration

```csharp
using Spur.EntityFrameworkCore;

public async Task<Result<User>> GetUserAsync(int id, CancellationToken ct)
{
    // FirstOrResultAsync returns Result<User> instead of throwing
    return await _db.Users
        .Where(u => u.Id == id)
        .FirstOrResultAsync(
            Error.NotFound($"User {id} not found", "USER_NOT_FOUND"),
            ct);
}

public async Task<Result<User>> UpdateUserAsync(User user, CancellationToken ct)
{
    _db.Users.Update(user);

    // SaveChangesResultAsync catches DbUpdateException → Conflict/Unexpected
    return await _db.SaveChangesResultAsync(ct)
        .Map(_ => user);
}
```

### 4. FluentValidation Integration

```csharp
using Spur.FluentValidation;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).InclusiveBetween(1, 150);
    }
}

public async Task<Result<User>> CreateUserAsync(
    CreateUserRequest request,
    IValidator<CreateUserRequest> validator,
    CancellationToken ct)
{
    return await Result.Start(request)
        .ValidateAsync(validator, ct)  // Automatic validation error → 422
        .ThenAsync(async req => await _repo.CreateAsync(req, ct));
}
```

### 5. MediatR/CQRS Integration

```csharp
using Spur.MediatR;

public record GetUserQuery(int UserId) : IRequest<Result<UserDto>>;

public class GetUserHandler : ResultHandler<GetUserQuery, UserDto>
{
    protected override async Task<Result<UserDto>> HandleAsync(
        GetUserQuery request,
        CancellationToken ct)
    {
        return await Result.Start(request.UserId)
            .ThenAsync(async id => await _repo.FindAsync(id, ct),
                Error.NotFound("User not found"))
            .Map(user => _mapper.Map<UserDto>(user));
    }
}
```

### 6. Unit Testing

```csharp
using Spur.Testing;

[Fact]
public async Task GetUser_WhenExists_ShouldReturnUser()
{
    var result = await _service.GetUserAsync(1, CancellationToken.None);

    result.ShouldBeSuccess()
        .WithValue(user => Assert.Equal("test@example.com", user.Email));
}

[Fact]
public async Task GetUser_WhenNotFound_ShouldReturn404()
{
    var result = await _service.GetUserAsync(999, CancellationToken.None);

    result.ShouldBeFailure()
        .WithCode("USER_NOT_FOUND")
        .WithHttpStatus(404)
        .WithCategory(ErrorCategory.NotFound);
}
```

## Use Cases & Package Selection

### Scenario 1: ASP.NET Core Web API
```bash
dotnet add package Spur
dotnet add package Spur.AspNetCore
dotnet add package Spur.FluentValidation
dotnet add package Spur.EntityFrameworkCore
```

### Scenario 2: Console Application / Business Logic Library
```bash
dotnet add package Spur
# That's it! No other dependencies needed
```

### Scenario 3: Blazor/MAUI Client
```bash
dotnet add package Spur
dotnet add package Spur.Testing  # For testing
```

### Scenario 4: CQRS Application with MediatR
```bash
dotnet add package Spur
dotnet add package Spur.MediatR
dotnet add package Spur.FluentValidation
dotnet add package Spur.AspNetCore  # If exposing HTTP API
```

### Scenario 5: Native AOT Deployment
```bash
dotnet add package Spur
dotnet add package Spur.AspNetCore
dotnet add package Spur.Generators  # Enhances AOT compatibility
```

## Pipeline Operators

### Core Operators

| Operator | Purpose | Example |
|----------|---------|---------|
| `Then` | Chain operations | `result.Then(x => x * 2)` |
| `ThenAsync` | Chain async operations | `result.ThenAsync(async x => await GetAsync(x))` |
| `Map` | Transform success value | `result.Map(user => user.Email)` |
| `MapAsync` | Transform async | `result.MapAsync(async x => await TransformAsync(x))` |
| `Validate` | Add validation | `result.Validate(x => x > 0, Error.Validation("Must be positive"))` |
| `ValidateAsync` | Async validation | `result.ValidateAsync(validator, ct)` |
| `Tap` | Side effects on success | `result.Tap(x => _logger.LogInfo($"Value: {x}"))` |
| `TapError` | Side effects on failure | `result.TapError(err => _logger.LogError(err.Message))` |
| `Recover` | Provide fallback | `result.Recover(error => defaultValue)` |
| `RecoverIf` | Conditional recovery | `result.RecoverIf(ErrorCategory.NotFound, _ => defaultUser)` |
| `Match` | Pattern match result | `result.Match(onSuccess: x => x, onFailure: _ => 0)` |

### Terminal Operations

```csharp
// Get value or throw
var value = result.Unwrap();

// Get value or default
var value = result.UnwrapOr(defaultValue);
var value = result.GetValueOrDefault();

// Convert to HTTP response
return result.ToHttpResult(mapper);

// Convert to MVC ActionResult
return result.ToActionResult(mapper);

// Pattern matching
var output = result.Match(
    onSuccess: value => $"Success: {value}",
    onFailure: error => $"Error: {error.Code}");
```

## Error Types

```csharp
// Built-in error factories
Error.Validation("Invalid input", "VALIDATION_ERROR");      // 422
Error.NotFound("Resource not found", "NOT_FOUND");          // 404
Error.Unauthorized("Access denied", "UNAUTHORIZED");         // 401
Error.Forbidden("Forbidden", "FORBIDDEN");                   // 403
Error.Conflict("Already exists", "CONFLICT");                // 409
Error.TooManyRequests("Rate limit exceeded", "RATE_LIMIT"); // 429
Error.Unexpected("System error", "UNEXPECTED_ERROR");        // 500

// Custom error with custom status code
Error.Custom(418, "I_AM_A_TEAPOT", "I'm a teapot", ErrorCategory.Custom);

// With extensions (additional metadata)
Error.Validation("Email is invalid")
    .WithExtensions(new { Field = "Email", Regex = @"^\S+@\S+$" });

// With inner error
Error.Unexpected("Database error")
    .WithInner(Error.Conflict("Unique constraint violation"));
```

## Configuration

### ASP.NET Core Setup

```csharp
// Program.cs
builder.Services.AddSpur(options =>
{
    // RFC 7807 Problem Details type URL prefix
    options.ProblemDetailsTypeBaseUri = "https://api.myapp.com/errors/";

    // Include error extensions in Problem Details response
    options.IncludeExtensions = true;

    // Include inner error details
    options.IncludeInnerErrors = true;

    // Custom status code mapping (optional)
    options.CustomStatusMapper = error => error.Category switch
    {
        ErrorCategory.Custom => error.HttpStatus,
        _ => null  // Use default
    };
});
```

## Performance

Spur is designed for zero-allocation success paths:

| Operation | Allocations | Speed vs Exception |
|-----------|-------------|-------------------|
| `Result.Success(value)` | 0 bytes | N/A |
| `Result.Failure(error)` | 0 bytes | N/A |
| 3-step pipeline (success) | 0 bytes | N/A |
| `Result` failure path | Minimal | 10-100× faster |

Run benchmarks:
```bash
dotnet run -c Release --project benchmarks/Spur.Benchmarks
```

## Roslyn Analyzers

Spur includes analyzers that catch common mistakes:

| Rule | Description |
|------|-------------|
| **RF0001** | Result value is ignored (must be used or stored) |
| **RF0002** | Unsafe access to `Result.Value` without checking `IsSuccess` |
| **RF0003** | Unsafe access to `Result.Error` without checking `IsFailure` |

## Global Usings (Recommended)

Add to `GlobalUsings.cs`:

```csharp
global using Spur;
global using Spur.Pipeline;

// Add only the packages you use:
global using Spur.AspNetCore;
global using Spur.EntityFrameworkCore;
global using Spur.FluentValidation;

// In test projects only:
global using Spur.Testing;
```

## Target Frameworks

- .NET 10.0 (primary)
- .NET 9.0
- .NET 8.0

## Sample Application

See the complete [sample application](samples/Spur.SampleApi) for a working CRUD API demonstrating all features.

```bash
cd samples/Spur.SampleApi
dotnet run
# API available at http://localhost:5000
```

## Documentation

- [API Reference](https://spur.adellajil.com/api-reference)
- [Migration Guide](https://spur.adellajil.com/migration)
- [Best Practices](https://spur.adellajil.com/best-practices)
- [Troubleshooting](https://spur.adellajil.com/troubleshooting)

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

Spur is inspired by Spur-Oriented Programming concepts from functional programming languages (F#, Rust, Haskell) and brings them idiomatically to .NET.
