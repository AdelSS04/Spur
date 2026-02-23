---
sidebar_position: 1
---

# Error API Reference

Complete reference for the `Error` type.

## Type definition

```csharp
public readonly record struct Error
```

`Error` is an immutable value type. All modification methods return new instances.

## Properties

| Property | Type | Description |
|---|---|---|
| `Code` | `string` | Machine-readable error code (e.g. `"USER_NOT_FOUND"`) |
| `Message` | `string` | Human-readable description |
| `HttpStatus` | `int` | HTTP status code (e.g. `404`) |
| `Category` | `ErrorCategory` | Semantic category (e.g. `NotFound`) |
| `Extensions` | `IReadOnlyDictionary<string, object?>` | Additional metadata |
| `Inner` | `Error?` | Optional inner error for chaining |

## Static factory methods

| Method | HTTP Status | Category |
|---|---|---|
| `Validation(message, code?)` | 422 | `Validation` |
| `Validation(code, message, extensions)` | 422 | `Validation` |
| `NotFound(message, code?)` | 404 | `NotFound` |
| `NotFound(code, message, extensions)` | 404 | `NotFound` |
| `Unauthorized(message, code?)` | 401 | `Unauthorized` |
| `Unauthorized(code, message, extensions)` | 401 | `Unauthorized` |
| `Forbidden(message, code?)` | 403 | `Forbidden` |
| `Forbidden(code, message, extensions)` | 403 | `Forbidden` |
| `Conflict(message, code?)` | 409 | `Conflict` |
| `Conflict(code, message, extensions)` | 409 | `Conflict` |
| `TooManyRequests(message, code?)` | 429 | `TooManyRequests` |
| `TooManyRequests(code, message, extensions)` | 429 | `TooManyRequests` |
| `Unexpected(message, code?)` | 500 | `Unexpected` |
| `Unexpected(exception, code?)` | 500 | `Unexpected` |
| `Custom(httpStatus, code, message, category?)` | custom | `Custom` |
| `Custom(httpStatus, code, message, extensions, category?)` | custom | `Custom` |

## Instance methods

| Method | Returns | Description |
|---|---|---|
| `WithMessage(message)` | `Error` | New Error with a different message |
| `WithCode(code)` | `Error` | New Error with a different code |
| `WithInner(error)` | `Error` | New Error with an inner error attached |
| `WithExtensions(extensions)` | `Error` | New Error with merged extensions |

## Static default instances

| Field | Code | HTTP Status |
|---|---|---|
| `DefaultNotFound` | `NOT_FOUND` | 404 |
| `DefaultValidation` | `VALIDATION_ERROR` | 422 |
| `DefaultUnauthorized` | `UNAUTHORIZED` | 401 |
| `DefaultForbidden` | `FORBIDDEN` | 403 |
| `DefaultConflict` | `CONFLICT` | 409 |
| `DefaultUnexpected` | `UNEXPECTED_ERROR` | 500 |

## ErrorCategory enum

```csharp
public enum ErrorCategory
{
    Validation,      // 422
    NotFound,        // 404
    Unauthorized,    // 401
    Forbidden,       // 403
    Conflict,        // 409
    TooManyRequests, // 429
    Unexpected,      // 500
    Custom
}
```

## Examples

```csharp
// Simple error
var error = Error.NotFound("User not found", "USER_NOT_FOUND");

// Error with extensions
var error = Error.Validation("FIELD_ERRORS", "Validation failed", new
{
    fields = new[] { "email", "name" }
});

// Error with inner error
var outer = Error.Unexpected("Save failed", "SAVE_ERROR")
    .WithInner(Error.Unexpected("Connection timeout", "DB_TIMEOUT"));

// From an exception
try { /* ... */ }
catch (Exception ex)
{
    var error = Error.Unexpected(ex);
    // error.Extensions["exceptionType"] == "InvalidOperationException"
}
```

## See also

- [Error type guide](../core-concepts/error-type) — conceptual explanation
- [Result&lt;T&gt; API](./result) — using errors with Results
