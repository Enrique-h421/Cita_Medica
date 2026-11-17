using Catalogos.Api.Middleware;
using Catalogos.Core;
using Catalogos.Persistence;
using GenericPersistence.Repository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// ---------------- Base de datos y repositorio genérico ----------------
builder.Services.AddDbContext<CatalogosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogosConnection")));

// Se reutiliza la misma librería de persistencia genérica del otro microservicio:
// Catálogos no escribe ni un repositorio propio.
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<CatalogosDbContext>());
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// ---------------- CORS para el frontend de Angular ----------------
const string CorsAngular = "CorsAngular";

var origenesPermitidos = builder.Configuration
    .GetSection("Cors:OrigenesPermitidos")
    .Get<string[]>() ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsAngular, policy => policy
        .WithOrigins(origenesPermitidos)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// ---------------- MVC y serialización ----------------
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddCatalogosCore();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CitaMedica - Catalogos v1");
        options.DocumentTitle = "CitaMedica - Catálogos y Seguridad";
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsAngular);
app.UseAuthorization();
app.MapControllers();

app.Run();
