---
sidebar_position: 1
---

# Then / ThenAsync

`Then` chains an operation that **can fail**. It only runs if the previous Result was successful; otherwise it short-circuits and passes the error along.

## Signatures

```csharp
// Sync
Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> next)

// With null-check (reference types)
Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut?> next, Error onNull)

// Async
Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> next)
Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result<TOut>>> next)
Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Result<TOut>> next)
```

## When to use

Use `Then` when the next step **returns a Result** — i.e. it can succeed or fail.

- Database lookups
- External API calls
- Any operation that might produce an error

If the transformation **cannot fail**, use [Map](./map) instead.

## Basic example

```csharp
public Result<User> GetActiveUser(int userId)
{
    return GetUser(userId)                  // Result<User>
        .Then(ValidateUserIsActive)          // only runs if GetUser succeeded
        .Then(CheckUserHasPermissions);      // only runs if validation passed
}

private Result<User> GetUser(int id)
{
    var user = _db.Users.Find(id);
    return user is not null
        ? Result.Success(user)
        : Result.Failure<User>(Error.NotFound($"User {id} not found"));
}

private Result<User> ValidateUserIsActive(User user)
{
    return user.IsActive
        ? Result.Success(user)
        : Result.Failure<User>(Error.Validation("User is inactive"));
}
```

## Short-circuiting

When any step fails, every subsequent `Then` is skipped:

```csharp
var result = GetUser(999)                  // fails → NotFound
    .Then(ValidateUserIsActive)            // skipped
    .Then(CheckUserHasPermissions)         // skipped
    .Then(LoadUserPreferences);            // skipped

// result.Error.Code == "NOT_FOUND"
```

## Async operations

```csharp
public async Task<Result<OrderConfirmation>> ProcessOrder(CreateOrderRequest request)
{
    return await ValidateRequest(request)
        .ThenAsync(r => CreateOrderAsync(r))
        .ThenAsync(order => ReserveInventoryAsync(order))
        .ThenAsync(order => ChargePaymentAsync(order))
        .ThenAsync(order => SendConfirmationAsync(order));
}
```

## Null-check overload

When the next step returns a nullable value, provide an error for the null case:

```csharp
var result = Result.Start(userId)
    .Then(id => _db.Users.Find(id), Error.NotFound("User not found"));
```

If `Find` returns `null`, the Result becomes a failure with the specified error.

## Changing the type

`Then` can change the Result type at each step:

```csharp
public Result<OrderSummary> GetOrderSummary(int orderId)
{
    return GetOrder(orderId)           // Result<Order>
        .Then(LoadOrderItems)           // Result<OrderWithItems>
        .Then(CalculateTotals)          // Result<OrderTotals>
        .Then(CreateSummary);           // Result<OrderSummary>
}
```

## Best practices

**Keep functions small.** Each function passed to `Then` should do one thing.

```csharp
// Good
.Then(ValidatePaymentMethod)
.ThenAsync(ChargeCard)
.ThenAsync(SendReceipt)

// Avoid
.Then(ValidateAndChargeAndNotify)
```

**Don't use Then for pure transformations.** If the operation cannot fail, use `Map`:

```csharp
// Wrong — unnecessary Result wrapping
.Then(user => Result.Success(user.ToDto()))

// Right
.Map(user => user.ToDto())
```

## See also

- [Map](./map) — transform without failure
- [Validate](./validate) — assert a condition
- [Recover](./recover) — handle errors
