using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using PruebaTecnica.Application.Currencies;
using PruebaTecnica.Application.CurrencyConversion;

namespace PruebaTecnica.Tests;

public class CurrencyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateAuthorizedClient();

    [Fact]
    public async Task Listar_monedas_incluye_las_semilla()
    {
        var currencies = await _client.GetFromJsonAsync<List<CurrencyResponse>>("/currencies");

        Assert.NotNull(currencies);
        foreach (var code in new[] { "PYG", "USD", "EUR", "BRL" })
            Assert.Contains(currencies, c => c.Code == code);
    }

    [Theory]
    [InlineData("USD", "PYG", "100", "730000")]
    [InlineData("PYG", "USD", "730000", "100")]
    [InlineData("eur", "brl", "100", "607.692308")]
    public async Task Convertir_usa_la_formula_del_enunciado(string from, string to, string amount, string expected)
    {
        var response = await _client.PostAsJsonAsync("/currency/convert", new
        {
            fromCurrencyCode = from,
            toCurrencyCode = to,
            amount = decimal.Parse(amount, CultureInfo.InvariantCulture)
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertCurrencyResponse>();
        Assert.Equal(from.ToUpperInvariant(), result!.FromCurrency);
        Assert.Equal(to.ToUpperInvariant(), result.ToCurrency);
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), result.ConvertedAmount);
    }

    [Fact]
    public async Task Convertir_con_moneda_inexistente_devuelve_404()
    {
        var response = await _client.PostAsJsonAsync("/currency/convert",
            new { fromCurrencyCode = "XXX", toCurrencyCode = "PYG", amount = 100 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Convertir_con_datos_invalidos_devuelve_400()
    {
        var response = await _client.PostAsJsonAsync("/currency/convert",
            new { fromCurrencyCode = "", toCurrencyCode = "", amount = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var fields = await response.ReadValidationErrorFieldsAsync();
        Assert.Equal(["FromCurrencyCode", "ToCurrencyCode", "Amount"], fields);
    }

    [Fact]
    public async Task Crear_moneda_la_guarda_en_mayusculas_y_rechaza_duplicados()
    {
        var created = await _client.PostAsJsonAsync("/currencies", new { code = "ars", name = "Peso argentino", rateToBase = 7.5m });
        var duplicated = await _client.PostAsJsonAsync("/currencies", new { code = "ArS", name = "Otro", rateToBase = 8m });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var currency = await created.Content.ReadFromJsonAsync<CurrencyResponse>();
        Assert.Equal("ARS", currency!.Code);
        Assert.Equal("/currencies/ARS", created.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.Conflict, duplicated.StatusCode);
    }

    [Fact]
    public async Task Crear_moneda_con_rate_no_positivo_devuelve_400()
    {
        var response = await _client.PostAsJsonAsync("/currencies", new { code = "CLP", name = "Peso chileno", rateToBase = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
