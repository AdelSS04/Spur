using Microsoft.AspNetCore.Mvc;
using Spur.AspNetCore;
using Spur.EntityFrameworkCore;
using Spur.SampleApi.Domain;
using Spur.SampleApi.Infrastructure;

namespace Spur.SampleApi.Features.Users;

public static class GetUser
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
                ct);

        return result.ToHttpResult(mapper);
    }
}
