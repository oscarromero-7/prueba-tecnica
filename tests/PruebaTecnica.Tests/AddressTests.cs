using System.Net;
using System.Net.Http.Json;
using PruebaTecnica.Application.Addresses;

namespace PruebaTecnica.Tests;

public class AddressTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateAuthorizedClient();

    private static object ValidAddress => new
    {
        street = "Calle Falsa 123",
        city = "Asunción",
        country = "Paraguay",
        zipCode = "9999"
    };

    [Fact]
    public async Task Crear_direccion_devuelve_201_y_aparece_en_el_listado()
    {
        var user = await _client.CreateUserAsync();

        var response = await _client.PostAsJsonAsync($"/users/{user.Id}/addresses", ValidAddress);
        var address = await response.Content.ReadFromJsonAsync<AddressResponse>();
        var list = await _client.GetFromJsonAsync<List<AddressResponse>>($"/users/{user.Id}/addresses");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/addresses/{address!.Id}", response.Headers.Location?.ToString());
        Assert.Equal(user.Id, address.UserId);
        Assert.Single(list!, a => a.Id == address.Id);
    }

    [Fact]
    public async Task Crear_direccion_para_usuario_inexistente_devuelve_404()
    {
        var response = await _client.PostAsJsonAsync("/users/999999/addresses", ValidAddress);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Crear_direccion_sin_campos_obligatorios_devuelve_400()
    {
        var user = await _client.CreateUserAsync();

        var response = await _client.PostAsJsonAsync($"/users/{user.Id}/addresses",
            new { street = "", city = "", country = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var fields = await response.ReadValidationErrorFieldsAsync();
        Assert.Equal(["Street", "City", "Country"], fields);
    }

    [Fact]
    public async Task Editar_y_eliminar_direccion()
    {
        var user = await _client.CreateUserAsync();
        var created = await (await _client.PostAsJsonAsync($"/users/{user.Id}/addresses", ValidAddress))
            .Content.ReadFromJsonAsync<AddressResponse>();

        var put = await _client.PutAsJsonAsync($"/addresses/{created!.Id}",
            new { street = "Nueva 1", city = "Luque", country = "Paraguay" });
        var updated = await put.Content.ReadFromJsonAsync<AddressResponse>();
        var delete = await _client.DeleteAsync($"/addresses/{created.Id}");
        var deleteAgain = await _client.DeleteAsync($"/addresses/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.Equal("Luque", updated!.City);
        Assert.Null(updated.ZipCode);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteAgain.StatusCode);
    }

    [Fact]
    public async Task Eliminar_usuario_borra_sus_direcciones_en_cascada()
    {
        var user = await _client.CreateUserAsync();
        var address = await (await _client.PostAsJsonAsync($"/users/{user.Id}/addresses", ValidAddress))
            .Content.ReadFromJsonAsync<AddressResponse>();

        await _client.DeleteAsync($"/users/{user.Id}");
        var response = await _client.DeleteAsync($"/addresses/{address!.Id}");

        // Si la dirección hubiera sobrevivido, este DELETE devolvería 204.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
