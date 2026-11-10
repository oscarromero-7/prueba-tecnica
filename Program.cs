using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Application;
using PruebaTecnica.Common.Exceptions;
using PruebaTecnica.Common.Middleware;
using PruebaTecnica.Common.OpenApi;
using PruebaTecnica.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
        .AddInterceptors(new AuditInterceptor()));

builder.Services.AddApplication();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<ApiKeySecurityTransformer>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "PruebaTecnica v1"));

app.Run();
