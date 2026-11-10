using FluentValidation;
using Microsoft.AspNetCore.Identity;
using PruebaTecnica.Application.Users;
using PruebaTecnica.Application.Users.Commands;
using PruebaTecnica.Application.Users.Queries;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Domain;

namespace PruebaTecnica.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddScoped<ICommandHandler<CreateUserCommand, UserResponse>, CreateUserHandler>();
        services.AddScoped<ICommandHandler<UpdateUserCommand, UserResponse>, UpdateUserHandler>();
        services.AddScoped<ICommandHandler<DeleteUserCommand>, DeleteUserHandler>();
        services.AddScoped<IQueryHandler<GetUsersQuery, List<UserResponse>>, GetUsersHandler>();
        services.AddScoped<IQueryHandler<GetUserByIdQuery, UserResponse>, GetUserByIdHandler>();

        return services;
    }
}
