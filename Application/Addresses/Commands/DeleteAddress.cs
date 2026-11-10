using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Addresses.Commands;

public record DeleteAddressCommand(int Id);

public class DeleteAddressHandler(AppDbContext db) : ICommandHandler<DeleteAddressCommand>
{
    public async Task HandleAsync(DeleteAddressCommand command, CancellationToken cancellationToken)
    {
        var deleted = await db.Addresses
            .Where(a => a.Id == command.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
            throw new NotFoundException($"No existe la dirección con id {command.Id}.");
    }
}
