using System.Net;

namespace PruebaTecnica.Tests;

public class ApiKeyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Request_sin_api_key_devuelve_401()
    {
        var response = await factory.CreateClient().GetAsync("/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Request_con_api_key_incorrecta_devuelve_401()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-KEY", "incorrecta");

        var response = await client.GetAsync("/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Request_con_api_key_correcta_devuelve_200()
    {
        var response = await factory.CreateAuthorizedClient().GetAsync("/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/openapi/v1.json")]
    [InlineData("/swagger/index.html")]
    [InlineData("/health")]
    public async Task Rutas_publicas_no_requieren_api_key(string path)
    {
        var response = await factory.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
