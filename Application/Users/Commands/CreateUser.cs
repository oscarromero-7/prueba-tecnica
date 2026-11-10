using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Domain;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Users.Commands;

// Body exacto del enunciado: { "name", "email" }.
public record CreateUserCommand(string Name, string Email);

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).ValidEmail();
    }
}

public class CreateUserHandler(AppDbContext db) : ICommandHandler<CreateUserCommand, UserResponse>
{
    public async Task<UserResponse> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var email = User.NormalizeEmail(command.Email);

        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
            throw new ConflictException($"Ya existe un usuario con el email '{email}'.");

        var user = new User
        {
            Name = command.Name.Trim(),
            Email = email
        };

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            // Dos requests simultáneos con el mismo email pueden pasar el AnyAsync; el índice único lo frena.
            throw new ConflictException($"Ya existe un usuario con el email '{email}'.");
        }

        return UserResponse.FromEntity(user);
    }
}
