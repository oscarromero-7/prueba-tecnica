using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Currencies.Queries;

public record GetCurrenciesQuery;

public class GetCurrenciesHandler(AppDbContext db) : IQueryHandler<GetCurrenciesQuery, List<CurrencyResponse>>
{
    public async Task<List<CurrencyResponse>> HandleAsync(GetCurrenciesQuery query, CancellationToken cancellationToken)
    {
        // Se ordena por Code y no por RateToBase: SQLite no puede ordenar el decimal (guardado como TEXT).
        return await db.Currencies
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(CurrencyResponse.Projection)
            .ToListAsync(cancellationToken);
    }
}
