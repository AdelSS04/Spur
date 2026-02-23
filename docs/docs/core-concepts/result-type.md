---
sidebar_position: 1
---

# Result&lt;T&gt; Type

`Result<T>` is the core type of Spur. It represents an operation that either succeeds with a value of type `T` or fails with an `Error`.

## Definition

```csharp
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public T Value { get; }       // throws SpurException if failed
    public Error Error { get; }   // throws SpurException if succeeded
}
```

Because it's a `readonly struct`:

- There are zero heap allocations on the success path.
- It's stack-allocated, so it's extremely fast.
- It has value semantics (equality is by value).
- It can never be `null`.

## Creating Results

### Success

```csharp
// Explicit factory
var result = Result.Success(42);

// Implicit conversion
Result<int> result = 42;

// Pipeline entry point
var result = Result.Start(42);

// Void operations (no meaningful value)
var result = Result.Success(); // returns Result<Unit>
```

### Failure

```csharp
// From an Error value
var error = Error.NotFound("User not found");
Result<User> result = error; // implicit conversion

// Explicit factory
var result = Result.Failure<User>(error);

// Inline
var result = Result.Failure<User>("NOT_FOUND", "User not found", 404);
```

## Accessing the value

### IsSuccess / IsFailure

```csharp
var result = GetUser(123);

if (result.IsSuccess)
    Console.WriteLine(result.Value.Name);
else
    Console.WriteLine(result.Error.Message);
```

### GetValueOrDefault

Returns `default(T)` when the Result is a failure:

```csharp
var user = result.GetValueOrDefault(); // null if failed
```

### UnwrapOr

Provide an explicit fallback:

```csharp
var count = result.UnwrapOr(0); // 0 if failed
```

### UnwrapOrElse

Compute a fallback from the error:

```csharp
var user = result.UnwrapOrElse(err => User.Guest);
```

### Unwrap

Returns the value or throws `SpurException`:

```csharp
var user = result.Unwrap(); // throws if failed
```

### Match

Branch on success/failure and produce a new value:

```csharp
string message = result.Match(
    onSuccess: user => $"Hello, {user.Name}",
    onFailure: error => $"Error: {error.Message}"
);
```

### MatchAsync

Async version of Match:

```csharp
var html = await result.MatchAsync(
    onSuccess: async user => await RenderProfileAsync(user),
    onFailure: async error => await RenderErrorPageAsync(error)
);
```

### Switch / SwitchAsync

Like Match, but performs actions without returning a value:

```csharp
result.Switch(
    onSuccess: user => _logger.LogInformation("Found {Name}", user.Name),
    onFailure: error => _logger.LogWarning("Failed: {Code}", error.Code)
);
```

## Implicit conversions

You can return values or errors directly — Spur converts them for you:

```csharp
public Result<User> GetUser(int id)
{
    var user = _db.Users.Find(id);
    if (user is null)
        return Error.NotFound("User not found"); // implicit Error → Result<User>
    return user; // implicit User → Result<User>
}
```

## Try / TryAsync

Wrap code that might throw into a Result automatically:

```csharp
// Sync
var result = Result.Try(() => int.Parse(input));

// Async
var result = await Result.TryAsync(() => _http.GetAsync(url));
```

If the delegate throws, you get `Error.Unexpected` with the exception details.

## Pipeline operators

The real power of `Result<T>` is chaining operations:

```csharp
return await Result.Start(userId)
    .ThenAsync(id => _repo.FindAsync(id))
    .Validate(user => user.IsActive, Error.Validation("Inactive"))
    .Map(user => new UserDto(user))
    .TapError(err => _logger.LogError("{Code}: {Message}", err.Code, err.Message));
```

Each operator returns a new Result. If any step fails, the pipeline short-circuits.

See the [Pipeline Operators](../pipeline/then) section for details on each operator.

## Combining Results

### Combine (fail-fast)

Returns the first failure encountered:

```csharp
var combined = Result.Combine(result1, result2, result3);
// combined.Value is IReadOnlyList<T> if all succeed
```

### CombineAll (collect all errors)

Collects every failure into one aggregated error:

```csharp
var combined = Result.CombineAll(result1, result2, result3);
```

### Typed Combine

Combine Results of different types into a tuple:

```csharp
var combined = Result.Combine(userResult, orderResult);
// combined.Value is (User, Order) if both succeed
```

## Next steps

- [Error type](./error-type) — how errors work
- [Result-Oriented Programming](./Spur-oriented-programming) — the big picture
- [Pipeline operators](../pipeline/then) — composing operations
