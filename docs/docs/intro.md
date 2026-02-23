---
sidebar_position: 1
slug: /
---

# Introduction

Welcome to **Spur** — a modern error-handling library that brings Result-Oriented Programming to .NET.

## What is Spur?

Spur gives you a type-safe, fluent, and high-performance alternative to throwing exceptions for business logic errors. Instead of hidden failure paths and slow exception handling, you get explicit, compiler-enforced error handling with built-in HTTP semantics.

### Key features

- **Zero allocations** — `Result<T>` is a `readonly struct`, so the success path never touches the heap.
- **HTTP-first** — every `Error` carries an HTTP status code (404, 422, 500 …) out of the box.
- **Fluent pipeline** — chain operations with `Then`, `Map`, `Validate`, `Tap`, and `Recover`.
- **10–100× faster** — returning a failure Result is orders of magnitude faster than throwing an exception.
- **Type-safe** — the compiler forces callers to handle both success and failure.
- **Zero dependencies** — the core package has no external dependencies.
- **Roslyn analyzers** — catch mistakes at compile time.
- **Native AOT ready** — fully compatible with ahead-of-time compilation.

## Why Spur?

### The problem

Traditional .NET error handling has two broken patterns:

**Exceptions as control flow** ❌

```csharp
public async Task<UserDto> GetUser(int id)
{
    var user = await _repo.FindAsync(id);
    if (user == null)
        throw new NotFoundException(); // 1000× slower, invisible in signature
    return _mapper.Map<UserDto>(user);
}
```

**Primitive returns** ❌

```csharp
public async Task<(UserDto? User, string? Error, int StatusCode)> GetUser(int id)
{
    // No type safety, manual destructuring, easy to ignore errors
}
```

### The Spur way

```csharp
public async Task<Result<UserDto>> GetUser(int id)
{
    return await Result.Start(id)
        .ThenAsync(id => _repo.FindAsync(id),
            Error.NotFound("User not found"))
        .Validate(user => user.IsActive,
            Error.Validation("User is inactive"))
        .Map(user => _mapper.Map<UserDto>(user));
}
```

What you get:

- The type signature makes failures visible.
- The compiler forces callers to handle both outcomes.
- Failures are 10–100× faster than exceptions.
- HTTP status codes are built in.
- The code reads like a pipeline of steps.

## Quick start

### 1. Install the NuGet package

```bash
dotnet add package Spur
```

### 2. Write your first Result

```csharp
using Spur;

public Result<int> Divide(int numerator, int denominator)
{
    if (denominator == 0)
        return Error.Validation("Cannot divide by zero", "DIVISION_BY_ZERO");

    return Result.Success(numerator / denominator);
}

var result = Divide(10, 2);
if (result.IsSuccess)
    Console.WriteLine($"Result: {result.Value}"); // 5
else
    Console.WriteLine($"Error: {result.Error.Message}");
```

### 3. Add ASP.NET Core integration

```bash
dotnet add package Spur.AspNetCore
```

```csharp
using Spur.AspNetCore;

builder.Services.AddSpur();

app.MapGet("/users/{id}", async (int id, IUserRepository repo) =>
{
    return await repo.GetUserAsync(id)
        .ToHttpResult(); // 200 OK or RFC 7807 Problem Details
});
```

## What's next?

- [Installation](./getting-started/installation) — full package guide
- [Quick Start](./getting-started/quick-start) — build a real API in 10 minutes
- [Your First Result](./getting-started/your-first-result) — understand Result basics
