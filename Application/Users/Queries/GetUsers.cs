using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Users.Queries;

public record GetUsersQuery(bool? IsActive);

public class GetUsersHandler(AppDbContext db) : IQueryHandler<GetUsersQuery, List<UserResponse>>
{
    public async Task<List<UserResponse>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var users = db.Users.AsNoTracking();

        if (query.IsActive is not null)
            users = users.Where(u => u.IsActive == query.IsActive);

        return await users
            .OrderBy(u => u.Id)
            .Select(UserResponse.Projection)
            .ToListAsync(cancellationToken);
    }
}
