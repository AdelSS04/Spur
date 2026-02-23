using Microsoft.EntityFrameworkCore;
using Spur.AspNetCore;
using Spur.EntityFrameworkCore;
using Spur.SampleApi.Domain;
using Spur.SampleApi.Infrastructure;

namespace Spur.SampleApi.Features.Users;

public static class ListUsers
{
    public static async Task<IResult> Handle(
        AppDbContext db,
        IProblemDetailsMapper mapper,
        CancellationToken ct)
    {
        var result = await db.Users
            .OrderBy(u => u.CreatedAt)
            .ToResultListAsync(ct);

        return result.ToHttpResult(mapper);
    }
}
