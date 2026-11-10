using System.Net.Http.Json;
using System.Text.Json;
using PruebaTecnica.Application.Users;

namespace PruebaTecnica.Tests;

public static class TestHelpers
{
    public static string NewEmail() => $"{Guid.NewGuid():N}@test.com";

    public static async Task<UserResponse> CreateUserAsync(this HttpClient client, string? email = null)
    {
        var response = await client.PostAsJsonAsync("/users", new { name = "Usuario Test", email = email ?? NewEmail() });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserResponse>())!;
    }

    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.Clone();
    }

    public static async Task<string[]> ReadValidationErrorFieldsAsync(this HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).ToArray();
    }
}
