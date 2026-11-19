using System.Text;
using Catalogos.Api.Middleware;
using Catalogos.Api.Security;
using Catalogos.Core;
using Catalogos.Core.Configs;
using Catalogos.Core.Repositories;
using Catalogos.Core.Services;
using Catalogos.Persistence;
using Catalogos.Persistence.Repositories;
using GenericPersistence.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// ---------------- Base de datos y repositorios ----------------
builder.Services.AddDbContext<CatalogosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogosConnection")));

// Se reutiliza la misma librería de persistencia genérica del otro microservicio.
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<CatalogosDbContext>());
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Medico_Especialidad tiene llave compuesta, que el repositorio genérico no
// maneja: esa tabla lleva su propio repositorio específico.
builder.Services.AddScoped<IMedicoEspecialidadRepository, MedicoEspecialidadRepository>();

// ---------------- Autenticación ----------------
// Este microservicio es el que EMITE los tokens. Agenda solo los verifica.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Seccion));
builder.Services.AddScoped<IAuthTokenService, JwtTokenService>();

var jwt = builder.Configuration.GetSection(JwtOptions.Seccion).Get<JwtOptions>() ?? new JwtOptions();

// Mejor tronar al arrancar con un mensaje claro que al primer login con un 500.
if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
{
    throw new InvalidOperationException(
        "Falta la llave Jwt:Key en appsettings.json, o es muy corta. " +
        "HMAC-SHA256 necesita al menos 32 bytes.");
}

var llaveFirma = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Sin esto, la librería renombra los claims a URLs largas y
        // [Authorize(Roles = ...)] deja de encontrar el rol.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = llaveFirma,

            NameClaimType = "nombreUsuario",
            RoleClaimType = "role"
        };
    });

// Todo endpoint que no diga nada sobre autorización queda protegido. Así no
// depende de que alguien se acuerde de poner [Authorize] en cada controlador
// nuevo: lo que se olvida queda cerrado, no abierto.
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
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CitaMedica - Catalogos y Seguridad",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue el token que devuelve POST /api/Auth/login. Solo el token, sin escribir la palabra Bearer."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddOpenApi();

builder.Services.AddCatalogosCore();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    // AllowAnonymous porque la política de respaldo también alcanza a este
    // endpoint, y el documento de OpenAPI no es secreto.
    app.MapOpenApi().AllowAnonymous();

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

// El orden importa: primero se averigua quién sos, después si podés pasar.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
