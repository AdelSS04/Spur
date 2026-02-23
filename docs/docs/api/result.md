---
sidebar_position: 2
---

# Result&lt;T&gt; API Reference

Complete reference for `Result<T>` and the static `Result` class.

## Result&lt;T&gt; struct

```csharp
public readonly struct Result<T>
```

### Properties

| Property | Type | Description |
|---|---|---|
| `IsSuccess` | `bool` | `true` if the operation succeeded |
| `IsFailure` | `bool` | `true` if the operation failed |
| `Value` | `T` | The success value. Throws `SpurException` if failed. |
| `Error` | `Error` | The error. Throws `SpurException` if succeeded. |

### Methods

| Method | Returns | Description |
|---|---|---|
| `GetValueOrDefault(defaultValue?)` | `T?` | Value if success, otherwise `defaultValue` (or `default(T)`) |
| `GetErrorOrDefault(defaultError?)` | `Error` | Error if failure, otherwise `defaultError` (or `default`) |
| `Match(onSuccess, onFailure)` | `TResult` | Branch on success/failure, return a value |
| `MatchAsync(onSuccess, onFailure)` | `Task<TResult>` | Async branch |
| `Switch(onSuccess, onFailure)` | `void` | Branch on success/failure, perform actions |
| `SwitchAsync(onSuccess, onFailure)` | `Task` | Async action branch |
| `Unwrap()` | `T` | Value if success, throws `SpurException` if failed |
| `UnwrapOr(fallback)` | `T` | Value if success, otherwise `fallback` |
| `UnwrapOrElse(fallback)` | `T` | Value if success, otherwise compute fallback from error |

### Implicit conversions

```csharp
// Value → Result<T> (success)
Result<int> result = 42;

// Error → Result<T> (failure)
Result<int> result = Error.NotFound("Not found");
```

## Static Result class

### Success factories

| Method | Returns | Description |
|---|---|---|
| `Success<T>(value)` | `Result<T>` | Create a success with a value |
| `Success()` | `Result<Unit>` | Create a success with no value |

### Failure factories

| Method | Returns | Description |
|---|---|---|
| `Failure<T>(error)` | `Result<T>` | Create a failure from an `Error` |
| `Failure<T>(code, message, httpStatus?)` | `Result<T>` | Create a failure from components |

### Pipeline entry points

| Method | Returns | Description |
|---|---|---|
| `Start<T>(value)` | `Result<T>` | Begin a pipeline with a value |
| `StartAsync<T>(factory)` | `Task<Result<T>>` | Begin a pipeline with an async factory |

### Try wrappers

| Method | Returns | Description |
|---|---|---|
| `Try<T>(func)` | `Result<T>` | Execute, catch exceptions as `Error.Unexpected` |
| `TryAsync<T>(func)` | `Task<Result<T>>` | Async version |

### Combine

| Method | Returns | Description |
|---|---|---|
| `Combine<T>(results[])` | `Result<IReadOnlyList<T>>` | Fail-fast: returns first failure |
| `CombineAll<T>(results[])` | `Result<IReadOnlyList<T>>` | Collect all failures into one error |
| `Combine<T1,T2>(r1, r2)` | `Result<(T1,T2)>` | Combine two different types |
| `Combine<T1,T2,T3>(r1, r2, r3)` | `Result<(T1,T2,T3)>` | Combine three different types |

## Pipeline operators

All pipeline operators are extension methods in the `Spur.Pipeline` namespace.

| Operator | Purpose | Input | Output |
|---|---|---|---|
| `Then` | Chain fallible op | `Result<TIn>` | `Result<TOut>` |
| `ThenAsync` | Async chain | `Task<Result<TIn>>` | `Task<Result<TOut>>` |
| `Map` | Transform value | `Result<TIn>` | `Result<TOut>` |
| `MapAsync` | Async transform | `Task<Result<TIn>>` | `Task<Result<TOut>>` |
| `Validate` | Assert condition | `Result<T>` | `Result<T>` |
| `ValidateAsync` | Async assert | `Task<Result<T>>` | `Task<Result<T>>` |
| `Tap` | Side effect on success | `Result<T>` | `Result<T>` |
| `TapError` | Side effect on failure | `Result<T>` | `Result<T>` |
| `TapBoth` | Side effect on either | `Result<T>` | `Result<T>` |
| `TapAsync` | Async side effect | `Task<Result<T>>` | `Task<Result<T>>` |
| `Recover` | Handle error | `Result<T>` | `Result<T>` |
| `RecoverIf` | Handle specific category | `Result<T>` | `Result<T>` |
| `RecoverIfCode` | Handle specific code | `Result<T>` | `Result<T>` |
| `RecoverAsync` | Async recovery | `Task<Result<T>>` | `Task<Result<T>>` |
| `Match` | Terminal branch | `Result<T>` | `TResult` |
| `MatchAsync` | Async terminal | `Task<Result<T>>` | `Task<TResult>` |

## SpurException

```csharp
public sealed class SpurException : Exception
{
    public Error Error { get; }
}
```

Thrown when accessing `.Value` on a failed Result or `.Error` on a successful Result.

## See also

- [Result&lt;T&gt; guide](../core-concepts/result-type) — conceptual explanation
- [Error API](./error) — Error reference
- [Unit](./unit) — void-equivalent type
