using PruebaTecnica.Application.Currencies;
using PruebaTecnica.Application.Currencies.Commands;
using PruebaTecnica.Application.Currencies.Queries;
using PruebaTecnica.Application.CurrencyConversion;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Validation;

namespace PruebaTecnica.Endpoints;

public static class CurrencyEndpoints
{
    public static IEndpointRouteBuilder MapCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        var currencies = app.MapGroup("/currencies").WithTags("Currencies");

        currencies.MapGet("", async (
                IQueryHandler<GetCurrenciesQuery, List<CurrencyResponse>> handler,
                CancellationToken cancellationToken) =>
            TypedResults.Ok(await handler.HandleAsync(new GetCurrenciesQuery(), cancellationToken)));

        currencies.MapGet("/{code}", async (
                string code,
                IQueryHandler<GetCurrencyByCodeQuery, CurrencyResponse> handler,
                CancellationToken cancellationToken) =>
            TypedResults.Ok(await handler.HandleAsync(new GetCurrencyByCodeQuery(code), cancellationToken)))
            .ProducesProblem(StatusCodes.Status404NotFound);

        currencies.MapPost("", async (
                CreateCurrencyCommand command,
                ICommandHandler<CreateCurrencyCommand, CurrencyResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var currency = await handler.HandleAsync(command, cancellationToken);
                return TypedResults.Created($"/currencies/{currency.Code}", currency);
            })
            .WithValidation<CreateCurrencyCommand>()
            .ProducesProblem(StatusCodes.Status409Conflict);

        // Ruta en singular ("/currency/convert") tal como la pide el enunciado.
        app.MapPost("/currency/convert", async (
                ConvertCurrencyQuery query,
                IQueryHandler<ConvertCurrencyQuery, ConvertCurrencyResponse> handler,
                CancellationToken cancellationToken) =>
            TypedResults.Ok(await handler.HandleAsync(query, cancellationToken)))
            .WithTags("Currencies")
            .WithValidation<ConvertCurrencyQuery>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
