using PruebaTecnica.Application.Users;
using PruebaTecnica.Application.Users.Commands;
using PruebaTecnica.Application.Users.Queries;
using PruebaTecnica.Common.Cqrs;
using PruebaTecnica.Common.Validation;

namespace PruebaTecnica.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapPost("", async (
                CreateUserCommand command,
                ICommandHandler<CreateUserCommand, UserResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var user = await handler.HandleAsync(command, cancellationToken);
                return TypedResults.Created($"/users/{user.Id}", user);
            })
            .WithValidation<CreateUserCommand>()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("", async (
                bool? isActive,
                IQueryHandler<GetUsersQuery, List<UserResponse>> handler,
                CancellationToken cancellationToken) =>
            TypedResults.Ok(await handler.HandleAsync(new GetUsersQuery(isActive), cancellationToken)));

        group.MapGet("/{id:int}", async (
                int id,
                IQueryHandler<GetUserByIdQuery, UserResponse> handler,
                CancellationToken cancellationToken) =>
            TypedResults.Ok(await handler.HandleAsync(new GetUserByIdQuery(id), cancellationToken)))
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:int}", async (
                int id,
                UpdateUserRequest request,
                ICommandHandler<UpdateUserCommand, UserResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateUserCommand(id, request.Name, request.Email, request.IsActive!.Value);
                return TypedResults.Ok(await handler.HandleAsync(command, cancellationToken));
            })
            .WithValidation<UpdateUserRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:int}", async (
                int id,
                ICommandHandler<DeleteUserCommand> handler,
                CancellationToken cancellationToken) =>
            {
                await handler.HandleAsync(new DeleteUserCommand(id), cancellationToken);
                return TypedResults.NoContent();
            })
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
