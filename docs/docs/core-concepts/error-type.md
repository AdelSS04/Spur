---
sidebar_position: 2
---

# Error Type

The `Error` type represents a failure in Spur. Unlike exceptions, errors are lightweight values that carry structured information about what went wrong — including an HTTP status code.

## Structure

```csharp
public readonly record struct Error
{
    public string Code { get; }           // Machine-readable: "USER_NOT_FOUND"
    public string Message { get; }        // Human-readable: "User 123 not found"
    public int HttpStatus { get; }        // HTTP status: 404
    public ErrorCategory Category { get; } // Semantic grouping: NotFound
    public IReadOnlyDictionary<string, object?> Extensions { get; } // Extra metadata
    public Error? Inner { get; }          // Optional chained error
}
```

## Built-in factories

Every factory sets the HTTP status code and category automatically.

### Validation — 422

```csharp
var error = Error.Validation("Email is required", "EMAIL_REQUIRED");
```

### NotFound — 404

```csharp
var error = Error.NotFound("User 123 not found", "USER_NOT_FOUND");
```

### Unauthorized — 401

```csharp
var error = Error.Unauthorized("Invalid credentials", "INVALID_CREDENTIALS");
```

### Forbidden — 403

```csharp
var error = Error.Forbidden("Access denied", "ACCESS_DENIED");
```

### Conflict — 409

```csharp
var error = Error.Conflict("Email already exists", "EMAIL_EXISTS");
```

### TooManyRequests — 429

```csharp
var error = Error.TooManyRequests("Rate limit exceeded", "RATE_LIMIT");
```

### Unexpected — 500

```csharp
// From a message
var error = Error.Unexpected("Database connection failed", "DB_ERROR");

// From an exception (captures exception type in Extensions)
var error = Error.Unexpected(exception);
```

### Custom

For any HTTP status code not covered above:

```csharp
var error = Error.Custom(418, "I_AM_TEAPOT", "I'm a teapot");
```

## Error codes

Error codes should be:

- **SCREAMING_SNAKE_CASE**
- **Machine-readable** — parseable by API consumers
- **Unique** within your application

Good examples: `USER_NOT_FOUND`, `EMAIL_ALREADY_EXISTS`, `INSUFFICIENT_BALANCE`

Bad examples: `Error1`, `user-not-found`, `UserNotFound`

## Error messages

Messages should be:

- **Human-readable** — clear enough to show in a UI
- **Specific** — include IDs or values when helpful
- **Actionable** — tell the caller what to fix

```csharp
// Good
Error.NotFound($"User {userId} not found", "USER_NOT_FOUND")
Error.Validation("Email must be in format user@domain.com", "INVALID_EMAIL")

// Bad
Error.NotFound("Not found", "ERROR")
Error.Validation("Invalid", "VALIDATION_ERROR")
```

## Error categories

```csharp
public enum ErrorCategory
{
    Validation,      // 422 — input is invalid
    NotFound,        // 404 — resource doesn't exist
    Unauthorized,    // 401 — not authenticated
    Forbidden,       // 403 — not authorized
    Conflict,        // 409 — resource state conflict
    TooManyRequests, // 429 — rate limited
    Unexpected,      // 500 — server error
    Custom           // any other status code
}
```

## Extensions (metadata)

Attach extra data to an error using an anonymous object:

```csharp
var error = Error.Validation("VALIDATION_FAILED", "Multiple errors", new
{
    fields = new[] { "email", "name" },
    count = 2
});

// Access later
var fields = error.Extensions["fields"];
```

## Inner errors

Chain errors for context:

```csharp
var dbError = Error.Unexpected("Connection failed", "DB_CONNECTION_ERROR");
var error = Error.Unexpected("Cannot save user", "USER_SAVE_ERROR")
    .WithInner(dbError);

// error.Inner?.Code == "DB_CONNECTION_ERROR"
```

## Immutable modification

Errors are immutable. `With*` methods return new instances:

```csharp
var error = Error.NotFound("Not found", "NOT_FOUND")
    .WithMessage("User not found")
    .WithCode("USER_NOT_FOUND")
    .WithExtensions(new { retryable = false })
    .WithInner(otherError);
```

## Predefined defaults

Common errors are available as static fields for convenience:

```csharp
Error.DefaultNotFound       // 404 — "The requested resource was not found."
Error.DefaultValidation     // 422 — "One or more validation errors occurred."
Error.DefaultUnauthorized   // 401 — "Authentication is required."
Error.DefaultForbidden      // 403 — "You do not have permission."
Error.DefaultConflict       // 409 — "The operation conflicts with current state."
Error.DefaultUnexpected     // 500 — "An unexpected error occurred."
```

## HTTP mapping

When using `Spur.AspNetCore`, errors automatically convert to RFC 7807 Problem Details:

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

## Next steps

- [Result-Oriented Programming](./Spur-oriented-programming) — the big picture
- [ASP.NET Core integration](../integrations/aspnetcore) — HTTP mapping in detail
