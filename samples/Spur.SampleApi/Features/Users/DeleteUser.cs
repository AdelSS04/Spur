using Microsoft.AspNetCore.Mvc;
using Spur.AspNetCore;
using Spur.EntityFrameworkCore;
using Spur.Pipeline;
using Spur.SampleApi.Infrastructure;

namespace Spur.SampleApi.Features.Users;

public static class DeleteUser
{
    public static async Task<IResult> Handle(
        [FromRoute] int id,
        AppDbContext db,
        IProblemDetailsMapper mapper,
        CancellationToken ct)
    {
        var result = await db.Users
            .Where(u => u.Id == id)
            .FirstOrResultAsync(
                Error.NotFound($"User with ID {id} not found", "USER_NOT_FOUND"),
                ct)
            .ThenAsync(async user =>
            {
                db.Users.Remove(user);
                await db.SaveChangesResultAsync(ct);
                return Result.Success(Unit.Value);
            });

        return result.ToHttpResult(mapper);
    }
}
