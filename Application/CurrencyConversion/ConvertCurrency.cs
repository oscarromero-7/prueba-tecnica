using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Domain;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.CurrencyConversion;

// Es una query (no modifica datos) aunque el endpoint sea POST, porque recibe un body.
public record ConvertCurrencyQuery(string FromCurrencyCode, string ToCurrencyCode, decimal Amount);

public record ConvertCurrencyResponse(
    string FromCurrency,
    string ToCurrency,
    decimal OriginalAmount,
    decimal ConvertedAmount);

public class ConvertCurrencyValidator : AbstractValidator<ConvertCurrencyQuery>
{
    public ConvertCurrencyValidator()
    {
        RuleFor(x => x.FromCurrencyCode).NotEmpty();
        RuleFor(x => x.ToCurrencyCode).NotEmpty();

        // El tope evita overflow de decimal: amount (<= 1e12) * rate (< 1e12 por precision 18,6).
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(1_000_000_000_000m);
    }
}

public class ConvertCurrencyHandler(AppDbContext db)
    : IQueryHandler<ConvertCurrencyQuery, ConvertCurrencyResponse>
{
    public async Task<ConvertCurrencyResponse> HandleAsync(ConvertCurrencyQuery query, CancellationToken cancellationToken)
    {
        var fromCode = Currency.NormalizeCode(query.FromCurrencyCode);
        var toCode = Currency.NormalizeCode(query.ToCurrencyCode);

        var currencies = await db.Currencies
            .AsNoTracking()
            .Where(c => c.Code == fromCode || c.Code == toCode)
            .ToListAsync(cancellationToken);

        var from = currencies.FirstOrDefault(c => c.Code == fromCode)
            ?? throw new NotFoundException($"No existe la moneda '{fromCode}'.");
        var to = currencies.FirstOrDefault(c => c.Code == toCode)
            ?? throw new NotFoundException($"No existe la moneda '{toCode}'.");

        // Fórmula del enunciado: primero a la moneda base (PYG) y luego a la moneda destino.
        var baseAmount = query.Amount * from.RateToBase;
        var convertedAmount = Math.Round(baseAmount / to.RateToBase, 6);

        return new ConvertCurrencyResponse(from.Code, to.Code, query.Amount, convertedAmount);
    }
}
