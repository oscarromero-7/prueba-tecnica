using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Domain;
using PruebaTecnica.Infrastructure;

namespace PruebaTecnica.Application.Users.Commands;

public record CreateUserCommand(string Name, string Email, string? Password);

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).MinimumLength(6).MaximumLength(100).When(x => x.Password is not null);
    }
}

public class CreateUserHandler(AppDbContext db, IPasswordHasher<User> passwordHasher)
    : ICommandHandler<CreateUserCommand, UserResponse>
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

        if (command.Password is not null)
            user.PasswordHash = passwordHasher.HashPassword(user, command.Password);

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
