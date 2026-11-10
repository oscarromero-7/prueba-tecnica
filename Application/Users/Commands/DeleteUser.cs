using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Users.Commands;

public record DeleteUserCommand(int Id);

public class DeleteUserHandler(AppDbContext db) : ICommandHandler<DeleteUserCommand>
{
    public async Task HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        // Un solo DELETE en SQL; las direcciones se borran por el ON DELETE CASCADE de la FK.
        var deleted = await db.Users
            .Where(u => u.Id == command.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
            throw new NotFoundException($"No existe el usuario con id {command.Id}.");
    }
}
