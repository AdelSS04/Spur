---
sidebar_position: 3
---

# Unit API Reference

`Unit` is a void-equivalent type used when a `Result` carries no meaningful value.

## Definition

```csharp
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
```

## Static members

| Member | Type | Description |
|---|---|---|
| `Unit.Value` | `Unit` | The singleton instance |

## Equality & comparison

All `Unit` values are considered equal. Every comparison returns `0`.

```csharp
Unit.Value == default(Unit); // true
Unit.Value.CompareTo(default); // 0
```

## Operators

| Operator | Result |
|---|---|
| `==` | Always `true` |
| `!=` | Always `false` |

## Usage

`Unit` is used with `Result<Unit>` for operations that either succeed with no return value or fail:

```csharp
public Result<Unit> DeleteUser(int id)
{
    if (!_repo.Exists(id))
        return Error.NotFound("User.NotFound", "User not found");

    _repo.Delete(id);
    return Result.Success(); // returns Result<Unit>
}
```

The `Result.Success()` overload (no argument) returns `Result<Unit>`.

## See also

- [Result&lt;T&gt; API](./result)
- [Result type guide](../core-concepts/result-type)
