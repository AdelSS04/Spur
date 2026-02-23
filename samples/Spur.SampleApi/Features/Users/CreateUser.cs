using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Spur.AspNetCore;
using Spur.EntityFrameworkCore;
using Spur.FluentValidation;
using Spur.Pipeline;
using Spur.SampleApi.Domain;
using Spur.SampleApi.Infrastructure;

namespace Spur.SampleApi.Features.Users;

public static class CreateUser
{
    public record Request(string Email, string Name, int Age);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(255);
            RuleFor(x => x.Age).InclusiveBetween(1, 150);
        }
    }

    public static async Task<IResult> Handle(
        [FromBody] Request request,
        AppDbContext db,
        IValidator<Request> validator,
        IProblemDetailsMapper mapper,
        CancellationToken ct)
    {
        var result = await Result.Start(request)
            .ValidateAsync(validator, ct)
            .ThenAsync(async req =>
            {
                var user = new User
                {
                    Email = req.Email,
                    Name = req.Name,
                    Age = req.Age,
                    CreatedAt = DateTime.UtcNow
                };

                db.Users.Add(user);
                await db.SaveChangesResultAsync(ct);

                return Result.Success(user);
            });

        return result.ToHttpResultCreated(mapper, $"/users/{result.GetValueOrDefault()?.Id}");
    }
}
