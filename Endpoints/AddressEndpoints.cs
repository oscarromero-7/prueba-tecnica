using PruebaTecnica.Application.Addresses;
using PruebaTecnica.Application.Addresses.Commands;
using PruebaTecnica.Application.Addresses.Queries;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Validation;

namespace PruebaTecnica.Endpoints;

public static class AddressEndpoints
{
    public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var userAddresses = app.MapGroup("/users/{userId:int}/addresses").WithTags("Addresses");

        userAddresses.MapPost("", async (
                int userId,
                CreateAddressRequest request,
                ICommandHandler<CreateAddressCommand, AddressResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateAddressCommand(userId, request.Street, request.City, request.Country, request.ZipCode);
                var address = await handler.HandleAsync(command, cancellationToken);
                return TypedResults.Created($"/addresses/{address.Id}", address);
            })
            .WithValidation<CreateAddressRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        userAddresses.MapGet("", async (
                int userId,
                IQueryHandler<GetAddressesByUserQuery, List<AddressResponse>> handler,
                CancellationToken cancellationToken) =>
            TypedResults.Ok(await handler.HandleAsync(new GetAddressesByUserQuery(userId), cancellationToken)))
            .ProducesProblem(StatusCodes.Status404NotFound);

        var addresses = app.MapGroup("/addresses").WithTags("Addresses");

        addresses.MapPut("/{id:int}", async (
                int id,
                UpdateAddressRequest request,
                ICommandHandler<UpdateAddressCommand, AddressResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateAddressCommand(id, request.Street, request.City, request.Country, request.ZipCode);
                return TypedResults.Ok(await handler.HandleAsync(command, cancellationToken));
            })
            .WithValidation<UpdateAddressRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        addresses.MapDelete("/{id:int}", async (
                int id,
                ICommandHandler<DeleteAddressCommand> handler,
                CancellationToken cancellationToken) =>
            {
                await handler.HandleAsync(new DeleteAddressCommand(id), cancellationToken);
                return TypedResults.NoContent();
            })
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
