using FluentValidation;

namespace PruebaTecnica.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Handlers CQRS: se registran explícitamente a medida que se agregan (fases 3 a 5).

        return services;
    }
}
