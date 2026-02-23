---
sidebar_position: 6
---

# Match / MatchAsync

`Match` is a **terminal operator** — it consumes the Result and produces a final value by branching on success or failure. Unlike other pipeline operators, Match does not return a Result.

## Signatures

```csharp
// On Result<T> (instance method)
TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess, Func<Error, Task<TResult>> onFailure)

// Pipeline extensions (for Task<Result<T>>)
Task<TResult> MatchAsync<T, TResult>(this Task<Result<T>> resultTask,
    Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
Task<TResult> MatchAsync<T, TResult>(this Task<Result<T>> resultTask,
    Func<T, Task<TResult>> onSuccess, Func<Error, Task<TResult>> onFailure)
```

## When to use

Use `Match` when you want to **exit the pipeline** and produce a final value:

- Building a response message
- Choosing a return value
- Rendering different UI

## Basic example

```csharp
var result = Divide(10, 2);

string message = result.Match(
    onSuccess: value => $"Answer: {value}",
    onFailure: error => $"Error: {error.Message}"
);

Console.WriteLine(message); // "Answer: 5"
```

## In a pipeline

```csharp
string response = await GetUserAsync(userId)
    .Validate(u => u.IsActive, Error.Validation("Inactive"))
    .Map(u => u.ToDto())
    .MatchAsync(
        onSuccess: dto => $"Welcome, {dto.Name}!",
        onFailure: err => $"Sorry: {err.Message}"
    );
```

## Switch / SwitchAsync

`Switch` is like `Match` but performs **actions** instead of returning a value:

```csharp
result.Switch(
    onSuccess: user => _logger.LogInformation("Found {Name}", user.Name),
    onFailure: error => _logger.LogWarning("Failed: {Code}", error.Code)
);

await result.SwitchAsync(
    onSuccess: async user => await _notifier.NotifyAsync(user),
    onFailure: async error => await _alerter.AlertAsync(error)
);
```

## Practical example — API response

```csharp
app.MapGet("/users/{id}", async (int id, IUserRepository repo, IProblemDetailsMapper mapper) =>
{
    return await repo.GetByIdAsync(id)
        .MatchAsync(
            onSuccess: user => Results.Ok(user),
            onFailure: error => Results.Problem(mapper.ToProblemDetails(error))
        );
});
```

:::tip
For ASP.NET Core, prefer `ToHttpResult()` from `Spur.AspNetCore` — it handles the Match + Problem Details mapping for you automatically.
:::

## See also

- [Then](./then) — chain fallible operations
- [Recover](./recover) — handle errors within the pipeline
- [Result&lt;T&gt; type](../core-concepts/result-type) — full reference
