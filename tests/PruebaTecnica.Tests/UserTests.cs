using System.Net;
using System.Net.Http.Json;
using PruebaTecnica.Application.Users;

namespace PruebaTecnica.Tests;

public class UserTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateAuthorizedClient();

    [Fact]
    public async Task Crear_usuario_devuelve_201_con_location_y_sin_password()
    {
        var email = TestHelpers.NewEmail().ToUpperInvariant();

        var response = await _client.PostAsJsonAsync("/users",
            new { name = "Juan", email, password = "secreta123" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal($"/users/{user!.Id}", response.Headers.Location?.ToString());
        Assert.Equal(email.ToLowerInvariant(), user.Email);
        Assert.True(user.IsActive);

        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Crear_usuario_invalido_devuelve_400_con_errores_por_campo()
    {
        var response = await _client.PostAsJsonAsync("/users", new { name = "", email = "no-es-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var fields = await response.ReadValidationErrorFieldsAsync();
        Assert.Contains("Name", fields);
        Assert.Contains("Email", fields);
    }

    [Fact]
    public async Task Email_duplicado_al_crear_devuelve_409_sin_distinguir_mayusculas()
    {
        var email = TestHelpers.NewEmail();
        await _client.CreateUserAsync(email);

        var response = await _client.PostAsJsonAsync("/users", new { name = "Otro", email = email.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Editar_con_email_de_otro_usuario_devuelve_409()
    {
        var existing = await _client.CreateUserAsync();
        var user = await _client.CreateUserAsync();

        var response = await _client.PutAsJsonAsync($"/users/{user.Id}",
            new { name = "Ana", email = existing.Email, isActive = true });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Editar_conservando_su_propio_email_devuelve_200()
    {
        var user = await _client.CreateUserAsync();

        var response = await _client.PutAsJsonAsync($"/users/{user.Id}",
            new { name = "Nombre Nuevo", email = user.Email, isActive = false });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("Nombre Nuevo", updated!.Name);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task Editar_sin_isActive_devuelve_400()
    {
        var user = await _client.CreateUserAsync();

        var response = await _client.PutAsJsonAsync($"/users/{user.Id}", new { name = "X", email = user.Email });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Filtrar_por_isActive_devuelve_solo_los_que_corresponden()
    {
        var inactive = await _client.CreateUserAsync();
        await _client.PutAsJsonAsync($"/users/{inactive.Id}",
            new { name = inactive.Name, email = inactive.Email, isActive = false });

        var users = await _client.GetFromJsonAsync<List<UserResponse>>("/users?isActive=false");

        Assert.NotNull(users);
        Assert.Contains(users, u => u.Id == inactive.Id);
        Assert.All(users, u => Assert.False(u.IsActive));
    }

    [Fact]
    public async Task Usuario_inexistente_devuelve_404()
    {
        var response = await _client.GetAsync("/users/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Eliminar_usuario_devuelve_204_y_luego_404()
    {
        var user = await _client.CreateUserAsync();

        var delete = await _client.DeleteAsync($"/users/{user.Id}");
        var get = await _client.GetAsync($"/users/{user.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Auditoria_completa_CreatedAt_y_UpdatedAt()
    {
        var createdAt = factory.Time.GetUtcNow().UtcDateTime;
        var user = await _client.CreateUserAsync();

        factory.Time.Advance(TimeSpan.FromHours(1));
        var response = await _client.PutAsJsonAsync($"/users/{user.Id}",
            new { name = "Editado", email = user.Email, isActive = true });
        var updated = await response.Content.ReadFromJsonAsync<UserResponse>();

        Assert.Equal(createdAt, user.CreatedAt);
        Assert.Null(user.UpdatedAt);
        Assert.Equal(createdAt, updated!.CreatedAt);
        Assert.Equal(createdAt.AddHours(1), updated.UpdatedAt);
        Assert.Equal(DateTimeKind.Utc, updated.UpdatedAt!.Value.Kind);
    }
}
