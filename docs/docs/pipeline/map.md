---
sidebar_position: 2
---

# Map / MapAsync

`Map` transforms the success value of a Result without changing whether it succeeded or failed. The transformation **cannot fail** — if it can, use [Then](./then) instead.

## Signatures

```csharp
// Sync
Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> transform)

// Async
Task<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> transform)
Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> transform)
Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<TOut>> transform)
```

## When to use

Use `Map` for transformations that **always succeed**:

- Converting an entity to a DTO
- Extracting a property from an object
- Formatting a string
- Any pure computation

**Do not use** `Map` when the operation can fail — use `Then`.
**Do not use** `Map` for side effects — use `Tap`.

## Basic example

```csharp
public Result<UserDto> GetUserDto(int userId)
{
    return GetUser(userId)
        .Map(user => new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        });
}
```

If `GetUser` fails, the `Map` is skipped and the original error passes through.

## Extracting a property

```csharp
Result<string> GetUserEmail(int userId)
{
    return GetUser(userId)
        .Map(user => user.Email);
}
```

## Chaining maps

```csharp
var result = Result.Start("  Hello, World!  ")
    .Map(s => s.Trim())
    .Map(s => s.ToUpperInvariant())
    .Map(s => s.Length);

// result.Value == 13
```

## Async map

```csharp
var result = await GetUserAsync(userId)
    .MapAsync(async user => await _avatarService.GetUrlAsync(user.Id));
```

## Map vs Then

| | Map | Then |
|---|---|---|
| Transformation can fail? | No | Yes |
| Return type of lambda | `TOut` | `Result<TOut>` |
| Use for | DTOs, formatting, projections | DB queries, API calls, validation |

```csharp
// Map — transformation always succeeds
.Map(user => user.ToDto())

// Then — operation can fail
.Then(user => ValidateAge(user))
```

## See also

- [Then](./then) — chain fallible operations
- [Validate](./validate) — assert conditions
- [Tap](./tap) — side effects
