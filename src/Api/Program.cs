using System.Text;
using Api.Middleware;
using Core;
using GenericPersistence.Repository;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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

// ---------------- Autenticación ----------------
// Agenda NO emite tokens: los emite Catálogos. Aquí solo se verifica la firma.
// Por eso los tres valores tienen que ser idénticos a los de Catálogos; si la
// llave difiere en un carácter, todos los tokens salen 401 sin explicación.
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CitaMedica.Catalogos";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CitaMedica";
var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;

if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Falta la llave Jwt:Key en appsettings.json, o es muy corta. " +
        "Tiene que ser la misma que usa el microservicio de Catálogos.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Sin esto, la librería renombra los claims a URLs largas y
        // [Authorize(Roles = ...)] deja de encontrar el rol.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            NameClaimType = "nombreUsuario",
            RoleClaimType = "role"
        };
    });

// Todo endpoint que no diga nada sobre autorización queda protegido: lo que se
// olvide queda cerrado, no abierto.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

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

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CitaMedica - Agenda y atencion",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue el token que devuelve POST /api/Auth/login en Catalogos (puerto 5110). Solo el token, sin escribir la palabra Bearer."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddOpenApi();

builder.Services.AddCore(builder.Configuration);            // opciones + MediatR
builder.Services.AddInfraestructure(builder.Configuration); // IRest + servicios HTTP

var app = builder.Build();

// Va de primero para que capture todo lo que truene más abajo.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    // AllowAnonymous porque la política de respaldo también alcanza a este
    // endpoint, y el documento de OpenAPI no es secreto.
    app.MapOpenApi().AllowAnonymous();

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

// El orden importa: primero se averigua quién sos, después si podés pasar.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
