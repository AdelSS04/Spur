---
sidebar_position: 3
---

# Your First Result

This page walks you through creating and consuming `Result<T>` values from scratch.

## The problem with exceptions

A typical C# method that throws:

```csharp
public int Divide(int numerator, int denominator)
{
    if (denominator == 0)
        throw new ArgumentException("Cannot divide by zero");
    return numerator / denominator;
}
```

What's wrong:

1. The signature doesn't reveal that this method can fail.
2. Throwing is ~1 000× slower than a normal return.
3. Callers can forget the `try`/`catch`.
4. Exceptions are not great for expected business logic failures.

## The Spur way

```csharp
using Spur;

public Result<int> Divide(int numerator, int denominator)
{
    if (denominator == 0)
        return Error.Validation("Cannot divide by zero", "DIVISION_BY_ZERO");

    return Result.Success(numerator / denominator);
}
```

The return type tells every caller that this method can succeed or fail, and the compiler helps you handle both cases.

## Consuming a Result

```csharp
var result = Divide(10, 2);

if (result.IsSuccess)
{
    Console.WriteLine($"Answer: {result.Value}"); // 5
}
else
{
    Console.WriteLine($"Error: {result.Error.Message}");
    Console.WriteLine($"Code:  {result.Error.Code}");
    Console.WriteLine($"HTTP:  {result.Error.HttpStatus}");
}
```

## Creating success values

```csharp
// Explicit factory
var r1 = Result.Success(42);

// Implicit conversion — just return the value
Result<int> r2 = 42;

// Pipeline entry point
var r3 = Result.Start(42);
```

## Creating failures

Use the `Error` factories. Each one sets the appropriate HTTP status code automatically.

```csharp
// Validation error → 422
Result<User> r1 = Error.Validation("Invalid email", "INVALID_EMAIL");

// Not found → 404
Result<User> r2 = Error.NotFound("User not found", "USER_NOT_FOUND");

// Conflict → 409
Result<User> r3 = Error.Conflict("Email taken", "EMAIL_EXISTS");

// Server error → 500
Result<User> r4 = Error.Unexpected("Database failed", "DB_ERROR");
```

## Safe value access

Accessing `.Value` on a failed Result throws `SpurException`. Use these patterns instead:

```csharp
var result = Divide(10, 0); // fails

// ✅ Check first
if (result.IsSuccess)
{
    var val = result.Value;
}

// ✅ Provide a fallback
var val = result.UnwrapOr(0); // returns 0 on failure

// ✅ Provide a fallback from the error
var val = result.UnwrapOrElse(err => -1);

// ✅ Default value
var val = result.GetValueOrDefault(); // returns default(int) = 0

// ✅ Pattern match
var message = result.Match(
    onSuccess: v => $"Answer: {v}",
    onFailure: e => $"Error: {e.Message}"
);
```

## Result&lt;T&gt; is a struct

`Result<T>` is a `readonly struct`, which means:

- **Zero heap allocations** on the success path.
- **Extremely fast** — stack-allocated.
- **Value semantics** — equality is by value, not reference.
- It can never be `null`.

## Pattern matching

C# pattern matching works naturally:

```csharp
var message = result switch
{
    { IsSuccess: true } => $"Value: {result.Value}",
    { IsFailure: true } => $"Error: {result.Error.Code}",
};

// Or use the Match method
var message = result.Match(
    onSuccess: v => $"Value: {v}",
    onFailure: e => $"Error: {e.Code}"
);
```

## Async Results

Results work seamlessly with `async`/`await`:

```csharp
public async Task<Result<User>> GetUserAsync(int id)
{
    var user = await _db.Users.FindAsync(id);

    if (user is null)
        return Error.NotFound($"User {id} not found", "USER_NOT_FOUND");

    return Result.Success(user);
}
```

## Composing Results

Chain operations into pipelines — if any step fails, the rest are skipped:

```csharp
public async Task<Result<UserDto>> GetActiveUser(int id)
{
    return await GetUserAsync(id)
        .Validate(user => user.IsActive,
            Error.Validation("User is inactive", "USER_INACTIVE"))
        .Map(user => new UserDto(user.Id, user.Name, user.Email));
}
```

## Combining multiple Results

```csharp
var r1 = Divide(10, 2);
var r2 = Divide(20, 4);

// All must succeed — fails fast on the first error
var combined = Result.Combine(r1, r2);
if (combined.IsSuccess)
{
    IReadOnlyList<int> values = combined.Value; // [5, 10]
}
```

## Best practices

1. **Use `Result<T>` for expected failures** — validation errors, not-found, conflicts. Keep exceptions for truly unexpected situations.
2. **Use consistent error codes** — `SCREAMING_SNAKE_CASE` (e.g. `USER_NOT_FOUND`).
3. **Write descriptive messages** — they'll appear in API responses.
4. **Prefer pipeline operators** over manual `if`/`else` checks.
5. **Never throw exceptions from Result-returning methods** — use `Error.Unexpected` instead.

## Next steps

- [Error types](../core-concepts/error-type) — learn about `Error` and `ErrorCategory` in depth
- [Result&lt;T&gt; type](../core-concepts/result-type) — full reference for `Result<T>`
- [Pipeline operators](../pipeline/then) — chain operations fluently
