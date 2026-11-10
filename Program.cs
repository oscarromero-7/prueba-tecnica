using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Application;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Common.Json;
using PruebaTecnica.Common.Middleware;
using PruebaTecnica.Common.OpenApi;
using PruebaTecnica.Endpoints;
using PruebaTecnica.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<AuditInterceptor>();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
        .AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>()));

builder.Services.AddApplication();

var timeZone = TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration["TimeZoneId"] ?? "America/Asuncion");
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new TimeZoneDateTimeConverter(timeZone)));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<ApiKeySecurityTransformer>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapUserEndpoints();
app.MapAddressEndpoints();
app.MapCurrencyEndpoints();

app.MapHealthChecks("/health");

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "PruebaTecnica v1"));

app.Run();
