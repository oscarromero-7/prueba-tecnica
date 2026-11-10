using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Domain;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Currencies.Commands;

public record CreateCurrencyCommand(string Code, string Name, decimal RateToBase);

public class CreateCurrencyValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RateToBase)
            .GreaterThan(0)
            .PrecisionScale(18, 6, ignoreTrailingZeros: true);
    }
}

public class CreateCurrencyHandler(AppDbContext db) : ICommandHandler<CreateCurrencyCommand, CurrencyResponse>
{
    public async Task<CurrencyResponse> HandleAsync(CreateCurrencyCommand command, CancellationToken cancellationToken)
    {
        var code = Currency.NormalizeCode(command.Code);

        if (await db.Currencies.AnyAsync(c => c.Code == code, cancellationToken))
            throw new ConflictException($"Ya existe la moneda '{code}'.");

        var currency = new Currency
        {
            Code = code,
            Name = command.Name.Trim(),
            RateToBase = command.RateToBase
        };

        db.Currencies.Add(currency);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            throw new ConflictException($"Ya existe la moneda '{code}'.");
        }

        return CurrencyResponse.FromEntity(currency);
    }
}
