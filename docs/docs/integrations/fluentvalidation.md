---
sidebar_position: 3
---

# FluentValidation

The `Spur.FluentValidation` package bridges FluentValidation validators with Spur pipelines, converting validation failures into structured `Error` values.

## Installation

```bash
dotnet add package Spur.FluentValidation
```

## Standalone usage

Run a validator and get a `Result<T>` back:

```csharp
using Spur.FluentValidation;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

var validator = new CreateUserValidator();
var result = validator.ValidateToResult(request);

if (result.IsFailure)
{
    // result.Error contains structured validation details
}
```

### Async

```csharp
var result = await validator.ValidateToResultAsync(request);
```

## Pipeline integration

Plug FluentValidation directly into a Spur pipeline:

```csharp
public async Task<Result<UserDto>> CreateUser(CreateUserRequest request)
{
    return await Result.Start(request)
        .Validate(_validator)                 // FluentValidation runs here
        .ThenAsync(r => _repo.CreateAsync(r))
        .Map(user => user.ToDto());
}
```

### Async pipeline

```csharp
return await Result.Start(request)
    .ValidateAsync(_validator, cancellationToken)
    .ThenAsync(r => _repo.CreateAsync(r));
```

### From Task&lt;Result&lt;T&gt;&gt;

```csharp
return await GetRequestAsync()
    .ValidateAsync(_validator, cancellationToken)
    .ThenAsync(ProcessAsync);
```

## Error format

When validation fails, the error has:

- **Code**: `"VALIDATION_ERROR"`
- **Message**: `"One or more validation errors occurred."`
- **HttpStatus**: `422`
- **Category**: `ErrorCategory.Validation`
- **Extensions**: contains a structured `errors` dictionary with property names and their messages:

```json
{
  "errorCode": "VALIDATION_ERROR",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "Email": ["'Email' must not be empty.", "'Email' is not a valid email address."],
    "Name": ["'Name' must not be empty."]
  }
}
```

## See also

- [Validate operator](../pipeline/validate) — simple boolean validation
- [Error type](../core-concepts/error-type) — error structure
- [ASP.NET Core](./aspnetcore) — HTTP response mapping
