using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Currencies.Queries;

public record GetCurrencyByCodeQuery(string Code);

public class GetCurrencyByCodeHandler(AppDbContext db) : IQueryHandler<GetCurrencyByCodeQuery, CurrencyResponse>
{
    public async Task<CurrencyResponse> HandleAsync(GetCurrencyByCodeQuery query, CancellationToken cancellationToken)
    {
        var code = query.Code.Trim().ToUpperInvariant();

        return await db.Currencies
            .AsNoTracking()
            .Where(c => c.Code == code)
            .Select(CurrencyResponse.Projection)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"No existe la moneda '{code}'.");
    }
}
