using System.Linq.Expressions;
using PruebaTecnica.Domain;

namespace PruebaTecnica.Application.Currencies;

public record CurrencyResponse(
    int Id,
    string Code,
    string Name,
    decimal RateToBase,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static readonly Expression<Func<Currency, CurrencyResponse>> Projection = c =>
        new CurrencyResponse(c.Id, c.Code, c.Name, c.RateToBase, c.CreatedAt, c.UpdatedAt);

    private static readonly Func<Currency, CurrencyResponse> Map = Projection.Compile();

    public static CurrencyResponse FromEntity(Currency currency) => Map(currency);
}
