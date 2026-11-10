using FluentValidation;
using PruebaTecnica.Application.Addresses;
using PruebaTecnica.Application.Addresses.Commands;
using PruebaTecnica.Application.Addresses.Queries;
using PruebaTecnica.Application.Currencies;
using PruebaTecnica.Application.Currencies.Commands;
using PruebaTecnica.Application.Currencies.Queries;
using PruebaTecnica.Application.CurrencyConversion;
using PruebaTecnica.Application.Users;
using PruebaTecnica.Application.Users.Commands;
using PruebaTecnica.Application.Users.Queries;
using PruebaTecnica.Common.Cqrs;

namespace PruebaTecnica.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddScoped<ICommandHandler<CreateUserCommand, UserResponse>, CreateUserHandler>();
        services.AddScoped<ICommandHandler<UpdateUserCommand, UserResponse>, UpdateUserHandler>();
        services.AddScoped<ICommandHandler<DeleteUserCommand>, DeleteUserHandler>();
        services.AddScoped<IQueryHandler<GetUsersQuery, List<UserResponse>>, GetUsersHandler>();
        services.AddScoped<IQueryHandler<GetUserByIdQuery, UserResponse>, GetUserByIdHandler>();

        services.AddScoped<ICommandHandler<CreateAddressCommand, AddressResponse>, CreateAddressHandler>();
        services.AddScoped<ICommandHandler<UpdateAddressCommand, AddressResponse>, UpdateAddressHandler>();
        services.AddScoped<ICommandHandler<DeleteAddressCommand>, DeleteAddressHandler>();
        services.AddScoped<IQueryHandler<GetAddressesByUserQuery, List<AddressResponse>>, GetAddressesByUserHandler>();
        services.AddScoped<ICommandHandler<CreateCurrencyCommand, CurrencyResponse>, CreateCurrencyHandler>();
        services.AddScoped<IQueryHandler<GetCurrenciesQuery, List<CurrencyResponse>>, GetCurrenciesHandler>();        services.AddScoped<IQueryHandler<ConvertCurrencyQuery, ConvertCurrencyResponse>, ConvertCurrencyHandler>();

        return services;
    }
}
