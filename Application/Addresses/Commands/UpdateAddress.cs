using FluentValidation;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Addresses.Commands;

public record UpdateAddressRequest(string Street, string City, string Country, string? ZipCode = null);

public record UpdateAddressCommand(int Id, string Street, string City, string Country, string? ZipCode);

public class UpdateAddressValidator : AbstractValidator<UpdateAddressRequest>
{
    public UpdateAddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ZipCode).MaximumLength(20);
    }
}

public class UpdateAddressHandler(AppDbContext db) : ICommandHandler<UpdateAddressCommand, AddressResponse>
{
    public async Task<AddressResponse> HandleAsync(UpdateAddressCommand command, CancellationToken cancellationToken)
    {
        var address = await db.Addresses.FindAsync([command.Id], cancellationToken)
            ?? throw new NotFoundException($"No existe la dirección con id {command.Id}.");

        address.Street = command.Street.Trim();
        address.City = command.City.Trim();
        address.Country = command.Country.Trim();
        address.ZipCode = string.IsNullOrWhiteSpace(command.ZipCode) ? null : command.ZipCode.Trim();

        await db.SaveChangesAsync(cancellationToken);

        return AddressResponse.FromEntity(address);
    }
}
