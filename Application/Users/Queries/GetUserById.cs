using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Users.Queries;

public record GetUserByIdQuery(int Id);

public class GetUserByIdHandler(AppDbContext db) : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<UserResponse> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id == query.Id)
            .Select(UserResponse.Projection)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"No existe el usuario con id {query.Id}.");
    }
}
