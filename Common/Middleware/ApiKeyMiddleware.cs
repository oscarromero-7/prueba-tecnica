using System.Security.Cryptography;
using System.Text;

namespace PruebaTecnica.Common.Middleware;

public class ApiKeyMiddleware
{
    public const string HeaderName = "X-API-KEY";

    private static readonly string[] PublicPaths = ["/swagger", "/openapi", "/health"];

    private readonly RequestDelegate _next;
    private readonly byte[] _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;

        var apiKey = configuration["ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Falta configurar 'ApiKey' en appsettings.json.");

        _apiKey = Encoding.UTF8.GetBytes(apiKey);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsPublicPath(context.Request.Path) || HasValidApiKey(context.Request))
        {
            await _next(context);
            return;
        }

        await Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "No autorizado",
                detail: $"Falta el header {HeaderName} o su valor es incorrecto.")
            .ExecuteAsync(context);
    }

    private static bool IsPublicPath(PathString path) =>
        PublicPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));

    private bool HasValidApiKey(HttpRequest request)
    {
        if (!request.Headers.TryGetValue(HeaderName, out var provided))
            return false;

        // Comparación en tiempo constante para no filtrar información por timing.
        var providedBytes = Encoding.UTF8.GetBytes(provided.ToString());
        return CryptographicOperations.FixedTimeEquals(providedBytes, _apiKey);
    }
}
