using Api.Middleware;
using Core;
using GenericPersistence.Repository;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Persistence;
using Persistence.repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// ---------------- Base de datos y repositorios ----------------
builder.Services.AddDbContext<ClinicaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClinicaConnection")));

// GenericRepository<T> pide un DbContext base, así que se resuelve al ClinicaDbContext.
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ClinicaDbContext>());
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<ICitaRepository, CitaRepository>();

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

    // Cita -> EstadoCita -> Citas -> Cita -> ... Sin esto Newtonsoft entra en bucle infinito.
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddCore(builder.Configuration);            // opciones + MediatR
builder.Services.AddInfraestructure(builder.Configuration); // IRest + servicios HTTP

var app = builder.Build();

// Va de primero para que capture todo lo que truene más abajo.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Swagger UI en http://localhost:5104/swagger
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CitaMedica - Agenda v1");
        options.DocumentTitle = "CitaMedica - Agenda y atención";
    });
}
else
{
    // Solo fuera de desarrollo: en dev obliga a Angular a pelear con el certificado.
    app.UseHttpsRedirection();
}

app.UseCors(CorsAngular);
app.UseAuthorization();
app.MapControllers();

app.Run();