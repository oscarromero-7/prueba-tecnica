using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Addresses.Queries;

public record GetAddressByIdQuery(int Id);

public class GetAddressByIdHandler(AppDbContext db) : IQueryHandler<GetAddressByIdQuery, AddressResponse>
{
    public async Task<AddressResponse> HandleAsync(GetAddressByIdQuery query, CancellationToken cancellationToken)
    {
        return await db.Addresses
            .AsNoTracking()
            .Where(a => a.Id == query.Id)
            .Select(AddressResponse.Projection)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"No existe la dirección con id {query.Id}.");
    }
}
