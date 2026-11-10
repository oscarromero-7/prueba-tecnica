using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Addresses.Queries;

public record GetAddressesByUserQuery(int UserId);

public class GetAddressesByUserHandler(AppDbContext db)
    : IQueryHandler<GetAddressesByUserQuery, List<AddressResponse>>
{
    public async Task<List<AddressResponse>> HandleAsync(GetAddressesByUserQuery query, CancellationToken cancellationToken)
    {
        // 404 si el usuario no existe; lista vacía si existe pero no tiene direcciones.
        if (!await db.Users.AnyAsync(u => u.Id == query.UserId, cancellationToken))
            throw new NotFoundException($"No existe el usuario con id {query.UserId}.");

        return await db.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == query.UserId)
            .OrderBy(a => a.Id)
            .Select(AddressResponse.Projection)
            .ToListAsync(cancellationToken);
    }
}
