---
sidebar_position: 3
---

# Validate / ValidateAsync

`Validate` checks a condition on the success value. If the condition is false, the Result becomes a failure. The value itself is **not changed** — it either passes through or gets replaced by an error.

## Signatures

```csharp
// Sync — fixed error
Result<T> Validate<T>(this Result<T> result, Func<T, bool> predicate, Error errorIfFalse)

// Sync — error computed from the value
Result<T> Validate<T>(this Result<T> result, Func<T, bool> predicate, Func<T, Error> errorFactory)

// Async
Task<Result<T>> ValidateAsync<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Error errorIfFalse)
Task<Result<T>> ValidateAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Error errorIfFalse)
Task<Result<T>> ValidateAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, Error errorIfFalse)
```

## When to use

Use `Validate` for **business rules** that can be expressed as boolean conditions:

- Is the user active?
- Is the order total positive?
- Does the user have permission?

## Basic example

```csharp
public Result<User> EnsureUserCanLogin(User user)
{
    return Result.Success(user)
        .Validate(u => u.IsActive,
            Error.Validation("Account is disabled", "USER_INACTIVE"))
        .Validate(u => !u.IsLocked,
            Error.Validation("Account is locked", "USER_LOCKED"))
        .Validate(u => u.EmailConfirmed,
            Error.Validation("Email not confirmed", "EMAIL_NOT_CONFIRMED"));
}
```

If the first validation fails, the remaining ones are skipped (short-circuit behavior).

## Dynamic error messages

Use the error factory overload to include the value in the error:

```csharp
.Validate(
    order => order.Total > 0,
    order => Error.Validation($"Order total must be positive, got {order.Total}", "INVALID_TOTAL")
)
```

## In a pipeline

```csharp
public async Task<Result<Order>> PlaceOrder(PlaceOrderRequest request)
{
    return await Result.Start(request)
        .Validate(r => r.Items.Count > 0,
            Error.Validation("Order must have items", "NO_ITEMS"))
        .Validate(r => r.Items.Count <= 100,
            Error.Validation("Too many items", "TOO_MANY_ITEMS"))
        .ThenAsync(r => CreateOrderAsync(r));
}
```

## Async validation

When the check itself is asynchronous (e.g. checking a database):

```csharp
var result = await Result.Start(request)
    .ValidateAsync(
        async r => await _db.Users.AnyAsync(u => u.Email == r.Email),
        Error.NotFound("User not found", "USER_NOT_FOUND"));
```

## Validate vs Then

| | Validate | Then |
|---|---|---|
| Changes the value? | No | Yes |
| Returns | Same `Result<T>` | `Result<TOut>` |
| Use for | Guard conditions | Operations that produce a new value |

## See also

- [Then](./then) — chain fallible operations
- [Map](./map) — transform values
- [FluentValidation integration](../integrations/fluentvalidation) — complex validation
