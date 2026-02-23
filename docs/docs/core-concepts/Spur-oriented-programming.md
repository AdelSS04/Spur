---
sidebar_position: 3
---

# Result-Oriented Programming

Result-Oriented Programming (ROP) is a functional pattern that treats your application as a **railway with two tracks**: a success track and a failure track. Spur makes this pattern natural in C#.

## The railway metaphor

Imagine each operation in your code as a segment of railway track. Every segment has two possible outputs:

- **Success** — the train continues forward on the top track.
- **Failure** — the train switches to the bottom (error) track and skips all remaining operations.

```
  Success ──→ Then ──→ Validate ──→ Map ──→ ✅ Value
                ↘          ↘          ↘
  Failure ──────────────────────────────→ ❌ Error
```

Once on the failure track, the train stays there — no more processing happens. This is called **short-circuiting**.

## Why not exceptions?

| | Exceptions | Result&lt;T&gt; |
|---|---|---|
| **Visibility** | Hidden — not in the method signature | Explicit — `Result<T>` in the return type |
| **Performance** | ~1 000× slower (stack unwinding) | Zero-cost on success, 10–100× faster on failure |
| **Compiler help** | None — forgotten catch is invisible | Compiler enforces handling |
| **Control flow** | Non-local jumps | Linear, predictable |
| **HTTP mapping** | Manual middleware | Built-in status codes |

## A practical example

Traditional approach with exceptions:

```csharp
public async Task<OrderDto> PlaceOrder(PlaceOrderRequest request)
{
    var user = await _userRepo.GetByIdAsync(request.UserId)
        ?? throw new NotFoundException("User not found");

    if (!user.IsActive)
        throw new ValidationException("User is inactive");

    var product = await _productRepo.GetByIdAsync(request.ProductId)
        ?? throw new NotFoundException("Product not found");

    if (product.Stock < request.Quantity)
        throw new ValidationException("Insufficient stock");

    var order = new Order(user, product, request.Quantity);
    await _orderRepo.SaveAsync(order);

    return order.ToDto();
}
```

The same logic with Spur:

```csharp
public async Task<Result<OrderDto>> PlaceOrder(PlaceOrderRequest request)
{
    return await Result.Start(request)
        .ThenAsync(r => _userRepo.GetByIdAsync(r.UserId))
        .Validate(user => user.IsActive,
            Error.Validation("User is inactive", "USER_INACTIVE"))
        .ThenAsync(_ => _productRepo.GetByIdAsync(request.ProductId))
        .Validate(product => product.Stock >= request.Quantity,
            Error.Validation("Insufficient stock", "INSUFFICIENT_STOCK"))
        .MapAsync(product => CreateAndSaveOrder(request, product))
        .Map(order => order.ToDto());
}
```

Benefits:

- Every step that can fail returns `Result<T>`.
- If the user is not found, the rest of the pipeline is skipped.
- The compiler forces callers to handle both success and failure.
- No exception overhead on any failure path.

## Building blocks

Spur provides six pipeline operators — each one does exactly one job:

| Operator | Purpose | Changes value? | Can fail? |
|---|---|---|---|
| **Then** | Chain a fallible operation | ✅ Yes | ✅ Yes |
| **Map** | Transform the success value | ✅ Yes | ❌ No |
| **Validate** | Assert a condition | ❌ No | ✅ Yes |
| **Tap** | Side effect (logging, caching) | ❌ No | ❌ No |
| **Recover** | Handle an error, get back on track | ✅ Yes | Maybe |
| **Match** | Terminal — branch on success/failure | ✅ Yes | ❌ No |

## Composing pipelines

Small, focused functions compose into readable pipelines:

```csharp
public async Task<Result<UserDto>> RegisterUser(RegisterRequest request)
{
    return await Result.Start(request)
        .Validate(r => !string.IsNullOrWhiteSpace(r.Email),
            Error.Validation("Email is required", "EMAIL_REQUIRED"))
        .ThenAsync(r => _userRepo.EnsureEmailNotTaken(r.Email))
        .ThenAsync(r => _userRepo.CreateAsync(r))
        .TapAsync(user => _emailService.SendWelcomeAsync(user.Email))
        .Map(user => user.ToDto());
}
```

Each line is a self-contained step. Reading top to bottom tells you exactly what the operation does and where it can fail.

## When to use Result&lt;T&gt;

Use `Result<T>` for **expected, business-level failures**:

- User not found
- Validation errors
- Duplicate records
- Permission denied
- Rate limiting

Keep throwing exceptions for **truly unexpected failures**:

- Null reference bugs
- Out-of-memory
- Network timeouts (unless you want to handle them as Results)

## Next steps

- [Then](../pipeline/then) — chaining fallible operations
- [Map](../pipeline/map) — transforming success values
- [Validate](../pipeline/validate) — asserting conditions
- [Tap](../pipeline/tap) — side effects
- [Recover](../pipeline/recover) — error recovery
- [Match](../pipeline/match) — terminal branching
