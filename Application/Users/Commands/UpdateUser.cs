using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Users.Commands;

// Body del PUT. IsActive es bool? para que omitirlo sea un error y no un "false" silencioso.
public record UpdateUserRequest(string Name, string Email, bool? IsActive);

public record UpdateUserCommand(int Id, string Name, string Email, bool IsActive);

public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.IsActive).NotNull();
    }
}

public class UpdateUserHandler(AppDbContext db) : ICommandHandler<UpdateUserCommand, UserResponse>
{
    public async Task<UserResponse> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([command.Id], cancellationToken)
            ?? throw new NotFoundException($"No existe el usuario con id {command.Id}.");

        var email = command.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email && u.Id != command.Id, cancellationToken))
            throw new ConflictException($"Ya existe otro usuario con el email '{email}'.");

        user.Name = command.Name.Trim();
        user.Email = email;
        user.IsActive = command.IsActive;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            throw new ConflictException($"Ya existe otro usuario con el email '{email}'.");
        }

        return UserResponse.FromEntity(user);
    }
}
