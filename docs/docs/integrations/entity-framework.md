---
sidebar_position: 2
---

# Entity Framework Core

The `Spur.EntityFrameworkCore` package wraps common EF Core operations so they return `Result<T>` instead of throwing exceptions.

## Installation

```bash
dotnet add package Spur.EntityFrameworkCore
```

## Query extensions

All query extensions are on `IQueryable<T>` and work with any EF Core `DbSet`.

### FirstOrResultAsync

Returns the first matching record, or `Error.NotFound` if none:

```csharp
using Spur.EntityFrameworkCore;

var result = await _db.Users
    .Where(u => u.IsActive)
    .FirstOrResultAsync();
```

With a predicate:

```csharp
var result = await _db.Users
    .FirstOrResultAsync(u => u.Email == email);
```

With a custom error:

```csharp
var result = await _db.Users
    .FirstOrResultAsync(
        u => u.Email == email,
        notFoundError: Error.NotFound($"No user with email {email}", "USER_NOT_FOUND"));
```

### SingleOrResultAsync

Returns exactly one matching record. Fails if zero or more than one:

```csharp
var result = await _db.Users
    .SingleOrResultAsync(u => u.Id == userId);
```

### ExistsOrResultAsync

Checks if any record matches. Returns `Result<Unit>`:

```csharp
var result = await _db.Users
    .ExistsOrResultAsync(u => u.Email == email);
```

### ToResultListAsync

Loads all matching records into an `IReadOnlyList<T>`:

```csharp
var result = await _db.Products
    .Where(p => p.CategoryId == categoryId)
    .ToResultListAsync();
```

### ToResultListOrFailAsync

Like `ToResultListAsync`, but fails if the list is empty:

```csharp
var result = await _db.Orders
    .Where(o => o.UserId == userId)
    .ToResultListOrFailAsync(
        notFoundError: Error.NotFound("No orders found", "NO_ORDERS"));
```

## SaveChangesResultAsync

Wraps `SaveChangesAsync` and catches database exceptions:

```csharp
var result = await _db.SaveChangesResultAsync();
```

Exception mapping:

| Exception | Error |
|---|---|
| `DbUpdateConcurrencyException` | `Error.Conflict` |
| `DbUpdateException` (unique violation) | `Error.Conflict` |
| `DbUpdateException` (FK violation) | `Error.Validation` |
| Other `DbUpdateException` | `Error.Unexpected` |

## In a pipeline

```csharp
public async Task<Result<UserDto>> GetUserDto(int userId)
{
    return await _db.Users
        .FirstOrResultAsync(u => u.Id == userId)
        .Validate(u => u.IsActive,
            Error.Validation("User is inactive", "USER_INACTIVE"))
        .Map(u => new UserDto(u.Id, u.Name, u.Email));
}
```

## See also

- [Result&lt;T&gt; type](../core-concepts/result-type) — core Result reference
- [Pipeline operators](../pipeline/then) — composing operations
