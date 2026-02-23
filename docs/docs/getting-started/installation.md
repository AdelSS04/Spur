---
sidebar_position: 1
---

# Installation

Getting Spur into your .NET project takes a single command. This guide walks you through the core package and every optional integration.

## Prerequisites

- .NET 8.0 or later

## Core package

Every Spur project starts here. Zero external dependencies.

```bash
dotnet add package Spur
```

You now have access to `Result<T>`, `Error`, `Unit`, and all pipeline operators (`Then`, `Map`, `Validate`, `Tap`, `Recover`, `Match`).

## Integration packages

Install only the packages you need.

### Spur.AspNetCore

For web APIs built with ASP.NET Core (Minimal APIs or MVC).

```bash
dotnet add package Spur.AspNetCore
```

Gives you:

- `ToHttpResult()` / `ToActionResult()` — automatic conversion of `Result<T>` to HTTP responses.
- RFC 7807 Problem Details formatting for errors.
- `AddSpur()` service registration with configurable `SpurOptions`.
- `SpurMiddleware` for global exception-to-ProblemDetails conversion.

### Spur.EntityFrameworkCore

Result-returning wrappers around common EF Core queries.

```bash
dotnet add package Spur.EntityFrameworkCore
```

Gives you:

- `FirstOrResultAsync()` / `SingleOrResultAsync()` — query a single record, return `Result<T>` instead of throwing.
- `ExistsOrResultAsync()` — check existence, get `Result<Unit>`.
- `ToResultListAsync()` — load a list with error handling.
- `SaveChangesResultAsync()` — catch `DbUpdateException` and map it to the right `Error`.

### Spur.FluentValidation

Bridge between FluentValidation and Spur pipelines.

```bash
dotnet add package Spur.FluentValidation
```

Gives you:

- `validator.ValidateToResult(instance)` — run validation, get `Result<T>` back.
- `.Validate(validator)` pipeline extension — plug FluentValidation directly into a pipeline chain.
- Automatic aggregation of validation failures into a structured `Error` with field-level details.

### Spur.MediatR

CQRS integration for MediatR.

```bash
dotnet add package Spur.MediatR
```

Gives you:

- `ResultHandler<TRequest, TResponse>` — abstract base class for handlers returning `Result<T>`.
- `ResultHandler<TRequest>` — base class for commands that return `Result<Unit>`.
- `ResultPipelineBehavior` — catches unhandled exceptions and wraps them as `Result.Failure`.

### Spur.Testing

Fluent test assertions for any test framework (xUnit, NUnit, MSTest).

```bash
dotnet add package Spur.Testing
```

Gives you:

- `result.ShouldBeSuccess()` / `result.ShouldBeFailure()`
- `.WithValue(expected)`, `.WithCode("NOT_FOUND")`, `.WithHttpStatus(404)`
- `.WithCategory(ErrorCategory.Validation)`, `.WithMessageContaining("…")`
- Async variants: `ShouldBeSuccessAsync()`, `ShouldBeFailureAsync()`

### Spur.Generators

Source generators for Native AOT deployments.

```bash
dotnet add package Spur.Generators
```

Emits AOT-compatible code at build time for HTTP result conversions. No reflection needed.

### Spur.Analyzers

Roslyn analyzers that catch mistakes at compile time.

```bash
dotnet add package Spur.Analyzers
```

Catches:

- Ignored `Result<T>` return values.
- Unsafe `.Value` access without checking `.IsSuccess`.
- Unsafe `.Error` access on success results.

## Common setups

### Web API

```bash
dotnet add package Spur
dotnet add package Spur.AspNetCore
dotnet add package Spur.FluentValidation
dotnet add package Spur.EntityFrameworkCore
dotnet add package Spur.Analyzers
```

### Console app or class library

```bash
dotnet add package Spur
dotnet add package Spur.Analyzers
```

### CQRS with MediatR

```bash
dotnet add package Spur
dotnet add package Spur.AspNetCore
dotnet add package Spur.MediatR
dotnet add package Spur.FluentValidation
dotnet add package Spur.EntityFrameworkCore
```

### Native AOT

```bash
dotnet add package Spur
dotnet add package Spur.AspNetCore
dotnet add package Spur.Generators
```

## Verify the installation

```csharp
using Spur;

var result = Result.Success(42);
Console.WriteLine(result.IsSuccess); // True
Console.WriteLine(result.Value);     // 42
```

If that compiles and runs, you're ready to go.

## Next steps

- [Quick Start](./quick-start) — build an API in 10 minutes
- [Your First Result](./your-first-result) — learn the basics of `Result<T>`
