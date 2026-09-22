using Core;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.repositories;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ClinicaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClinicaConnection")));

builder.Services.AddScoped<ICitaRepository, CitaRepository>();
builder.Services.AddScoped(typeof(Core.GenericRepository.IGenericRepository<>), typeof(Persistence.GenericRepository.GenericRepository<>));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Core.Citas.Queries.GetCitasQuery).Assembly));
builder.Services.AddAutoMapper(cfg => { }, typeof(Core.Citas.Mappings.CitaProfile).Assembly);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();