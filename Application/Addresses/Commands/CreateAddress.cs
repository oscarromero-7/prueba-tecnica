using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Domain;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Addresses.Commands;

public record CreateAddressRequest(string Street, string City, string Country, string? ZipCode);

public record CreateAddressCommand(int UserId, string Street, string City, string Country, string? ZipCode);

public class CreateAddressValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ZipCode).MaximumLength(20);
    }
}

public class CreateAddressHandler(AppDbContext db) : ICommandHandler<CreateAddressCommand, AddressResponse>
{
    public async Task<AddressResponse> HandleAsync(CreateAddressCommand command, CancellationToken cancellationToken)
    {
        if (!await db.Users.AnyAsync(u => u.Id == command.UserId, cancellationToken))
            throw new NotFoundException($"No existe el usuario con id {command.UserId}.");

        var address = new Address
        {
            UserId = command.UserId,
            Street = command.Street.Trim(),
            City = command.City.Trim(),
            Country = command.Country.Trim(),
            ZipCode = string.IsNullOrWhiteSpace(command.ZipCode) ? null : command.ZipCode.Trim()
        };

        db.Addresses.Add(address);
        await db.SaveChangesAsync(cancellationToken);

        return AddressResponse.FromEntity(address);
    }
}
