using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Spur.AspNetCore;
using Spur.EntityFrameworkCore;
using Spur.FluentValidation;
using Spur.Pipeline;
using Spur.SampleApi.Domain;
using Spur.SampleApi.Infrastructure;

namespace Spur.SampleApi.Features.Users;

public static class UpdateUser
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
        [FromRoute] int id,
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
                var user = await db.Users
                    .Where(u => u.Id == id)
                    .FirstOrResultAsync(
                        Error.NotFound($"User with ID {id} not found", "USER_NOT_FOUND"),
                        ct);

                return user.Map(u =>
                {
                    u.Email = req.Email;
                    u.Name = req.Name;
                    u.Age = req.Age;
                    return u;
                });
            })
            .ThenAsync(async user =>
            {
                await db.SaveChangesResultAsync(ct);
                return Result.Success(user);
            });

        return result.ToHttpResult(mapper);
    }
}
