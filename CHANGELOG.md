# Changelog

All notable changes to Spur will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-23

### Added

#### Core Package (Spur)
- Initial implementation of `Result<T>` struct
- `Error` type with HTTP-first semantics
- `ErrorCategory` enum (Validation, NotFound, Unauthorized, Forbidden, Conflict, TooManyRequests, Unexpected, Custom)
- `Unit` type for void operations
- `SpurException` for unwrap failures
- Pipeline operators: Then, Map, Validate, Tap, TapError, Recover, Match
- Full async/await support with `Task<Result<T>>` extensions
- Static factory methods: `Result.Success`, `Result.Failure`, `Result.Try`
- Combination operators: `Result.Combine`, `Result.CombineAll`

#### ASP.NET Core Integration (Spur.AspNetCore)
- `ToHttpResult()` extension for Minimal APIs
- `ToActionResult()` extension for MVC controllers
- RFC 7807 Problem Details mapping
- Configurable `SpurOptions`
- Automatic HTTP status code mapping from errors
- TraceId inclusion in Problem Details

#### Entity Framework Core Integration (Spur.EntityFrameworkCore)
- `FirstOrResultAsync` query extension
- `SingleOrResultAsync` query extension
- `SaveChangesResultAsync` with automatic exception handling
- Conflict error mapping for constraint violations

#### FluentValidation Integration (Spur.FluentValidation)
- `ValidateAsync` pipeline extension
- `ValidateToResultAsync` conversion method
- Automatic aggregation of validation errors

#### MediatR Integration (Spur.MediatR)
- `ResultHandler<TRequest, TResponse>` base class
- `ResultPipelineBehavior` for automatic exception wrapping

#### Testing Package (Spur.Testing)
- `ShouldBeSuccess` assertion
- `ShouldBeFailure` assertion
- Fluent assertion context: `WithValue`, `WithCode`, `WithHttpStatus`, `WithCategory`

#### Tooling
- `Spur.Generators` — Source generators for AOT compatibility
- `Spur.Analyzers` — Roslyn analyzers (RF0001: ignored result, RF0002: unsafe Value access)

### Changed
- N/A (initial release)

### Deprecated
- N/A

### Removed
- N/A

### Fixed
- N/A

### Security
- N/A

[1.0.0]: https://github.com/AdelSS04/Spur/releases/tag/v1.0.0
