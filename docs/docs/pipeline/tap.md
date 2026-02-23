---
sidebar_position: 4
---

# Tap / TapError / TapBoth

The Tap operators perform **side effects** without changing the Result. The Result passes through untouched — Tap is for logging, caching, notifications, metrics, and similar concerns.

## Signatures

```csharp
// On success
Result<T> Tap<T>(this Result<T> result, Action<T> onSuccess)
Task<Result<T>> TapAsync<T>(this Result<T> result, Func<T, Task> onSuccess)
Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Action<T> onSuccess)
Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> onSuccess)

// On failure
Result<T> TapError<T>(this Result<T> result, Action<Error> onError)
Task<Result<T>> TapErrorAsync<T>(this Result<T> result, Func<Error, Task> onError)
Task<Result<T>> TapErrorAsync<T>(this Task<Result<T>> resultTask, Action<Error> onError)
Task<Result<T>> TapErrorAsync<T>(this Task<Result<T>> resultTask, Func<Error, Task> onError)

// On both
Result<T> TapBoth<T>(this Result<T> result, Action<T> onSuccess, Action<Error> onError)
```

## When to use

- Logging
- Caching
- Sending notifications
- Updating metrics
- Triggering events

**Never use Tap for** transformations (use `Map`), validation (use `Validate`), or operations that can fail (use `Then`).

## Tap — side effect on success

```csharp
public Result<User> GetUser(int userId)
{
    return _repository.GetById(userId)
        .Tap(user => _logger.LogInformation("Loaded user {Id}", user.Id))
        .Tap(user => _cache.Set($"user:{user.Id}", user));
}
```

## TapError — side effect on failure

```csharp
public async Task<Result<Order>> ProcessOrder(CreateOrderRequest request)
{
    return await ValidateOrder(request)
        .ThenAsync(CreateOrder)
        .ThenAsync(ChargePayment)
        .TapError(err => _logger.LogError("Order failed: {Code}", err.Code))
        .TapError(err => _metrics.Increment("orders.failed"));
}
```

## TapBoth — side effect on either outcome

```csharp
public async Task<Result<User>> RegisterUser(RegisterRequest request)
{
    return await CreateUser(request)
        .TapBoth(
            user => _logger.LogInformation("Registered {Email}", user.Email),
            error => _logger.LogWarning("Registration failed: {Code}", error.Code)
        );
}
```

## Async side effects

```csharp
return await GetOrder(orderId)
    .TapAsync(async order =>
        await _notificationService.NotifyOrderViewedAsync(order.Id))
    .TapAsync(async order =>
        await _cache.SetAsync($"order:{order.Id}", order));
```

## Common patterns

### Logging pipeline

```csharp
return await GetUser(userId)
    .Tap(u => _logger.LogDebug("Retrieved user {Id}", u.Id))
    .ThenAsync(UpdateUser)
    .Tap(u => _logger.LogDebug("Updated user {Id}", u.Id))
    .ThenAsync(SaveChanges)
    .Tap(_ => _logger.LogInformation("Saved changes"))
    .TapError(err => _logger.LogError("Failed: {Code}", err.Code));
```

### Caching

```csharp
return await _repository.GetByIdAsync(productId)
    .TapAsync(async product =>
        await _cache.SetAsync($"product:{productId}", product, TimeSpan.FromMinutes(15)));
```

## Best practices

1. **Keep side effects simple.** Don't put complex logic in Tap.
2. **Handle exceptions in side effects.** A failing notification shouldn't crash the pipeline.
3. **Use TapError for centralized error logging** at the end of a pipeline.

## See also

- [Then](./then) — chain fallible operations
- [Map](./map) — transform values
- [Recover](./recover) — handle errors
