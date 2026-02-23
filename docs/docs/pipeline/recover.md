---
sidebar_position: 5
---

# Recover / RecoverIf

The Recover operators let you **handle an error** and get back on the success track. If the Result is already successful, recovery is skipped.

## Signatures

```csharp
// Recover from any error
Result<T> Recover<T>(this Result<T> result, Func<Error, Result<T>> recovery)
Result<T> Recover<T>(this Result<T> result, Func<Error, T> recovery)

// Recover only from a specific category
Result<T> RecoverIf<T>(this Result<T> result, ErrorCategory category, Func<Error, Result<T>> recovery)

// Recover only from a specific error code
Result<T> RecoverIfCode<T>(this Result<T> result, string code, Func<Error, Result<T>> recovery)

// Async
Task<Result<T>> RecoverAsync<T>(this Task<Result<T>> resultTask, Func<Error, Task<Result<T>>> recovery)
Task<Result<T>> RecoverAsync<T>(this Result<T> result, Func<Error, Task<Result<T>>> recovery)
Task<Result<T>> RecoverIfAsync<T>(this Task<Result<T>> resultTask, ErrorCategory category, Func<Error, Task<Result<T>>> recovery)
```

## When to use

- Provide a fallback value when something is not found
- Retry an operation with different parameters
- Convert one type of error into another
- Use a cached value when the source is unavailable

## Recover from any error

```csharp
var result = GetUserFromCache(userId)
    .Recover(err => GetUserFromDatabase(userId));
```

If the cache lookup fails, the database lookup runs. If the cache succeeds, the database is never hit.

## Recover with a fallback value

```csharp
var result = GetUserPreferences(userId)
    .Recover(err => UserPreferences.Default);
```

## RecoverIf — recover from a specific category

```csharp
var result = GetUser(userId)
    .RecoverIf(ErrorCategory.NotFound, err =>
        Result.Success(User.Anonymous));
```

Only `NotFound` errors are recovered. Other errors (Validation, Unauthorized, etc.) pass through unchanged.

## RecoverIfCode — recover from a specific error code

```csharp
var result = GetProduct(productId)
    .RecoverIfCode("PRODUCT_DISCONTINUED", err =>
        GetReplacementProduct(productId));
```

## Async recovery

```csharp
var result = await GetUserFromCacheAsync(userId)
    .RecoverAsync(async err =>
        await GetUserFromDatabaseAsync(userId));
```

## Practical example

```csharp
public async Task<Result<Config>> GetConfig(string key)
{
    return await _cache.GetAsync<Config>(key)
        .RecoverIfAsync(ErrorCategory.NotFound, async err =>
        {
            var config = await _db.Configs.FirstOrResultAsync(
                c => c.Key == key);

            // Cache it for next time
            if (config.IsSuccess)
                await _cache.SetAsync(key, config.Value);

            return config;
        });
}
```

## See also

- [Then](./then) — chain fallible operations
- [Tap](./tap) — side effects
- [Match](./match) — terminal branching
