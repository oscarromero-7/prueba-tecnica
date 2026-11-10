using System.Linq.Expressions;
using PruebaTecnica.Domain;

namespace PruebaTecnica.Application.Users;

public record UserResponse(
    int Id,
    string Name,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    // EF traduce esta expresión a SQL: el SELECT trae solo estas columnas (nunca PasswordHash).
    public static readonly Expression<Func<User, UserResponse>> Projection = u =>
        new UserResponse(u.Id, u.Name, u.Email, u.IsActive, u.CreatedAt, u.UpdatedAt);

    private static readonly Func<User, UserResponse> Map = Projection.Compile();

    public static UserResponse FromEntity(User user) => Map(user);
}
