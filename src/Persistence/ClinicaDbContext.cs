using Domain.AgendaYAtencion;
using Domain.Organizacion;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    /// <summary>
    /// Mapea las tablas de ClinicaDB que este microservicio usa. La base trae el
    /// esquema completo entregado por el ingeniero; aquí solo se declaran las
    /// tablas del alcance de Agenda más sus dependencias estructurales.
    /// Los tipos se declaran explícitos (varchar vs nvarchar) porque las tablas
    /// ya existen y EF debe generar SQL que les calce.
    /// </summary>
    public class ClinicaDbContext : DbContext
    {
        public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options) { }

        public DbSet<Cita> Citas { get; set; }
        public DbSet<EstadoCita> Estados_Cita { get; set; }
        public DbSet<Consultorio> Consultorios { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Area> Areas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EstadoCita>().ToTable("Estados_Cita").HasKey(e => e.EstadoCitaID);
            modelBuilder.Entity<Cita>().ToTable("Citas").HasKey(c => c.CitaID);

            // ---------------- Sucursales ----------------
            modelBuilder.Entity<Sucursal>(e =>
            {
                e.ToTable("Sucursales");
                e.HasKey(x => x.SucursalID);
                e.Property(x => x.Codigo).HasColumnType("varchar(20)").IsRequired();
                e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
                e.Property(x => x.Direccion).HasMaxLength(300);
                e.Property(x => x.Telefono).HasColumnType("varchar(30)");
                e.Property(x => x.Email).HasColumnType("varchar(150)");
            });

            // ---------------- Areas ----------------
            modelBuilder.Entity<Area>(e =>
            {
                e.ToTable("Areas");
                e.HasKey(x => x.AreaID);
                e.Property(x => x.Codigo).HasColumnType("varchar(30)").IsRequired();
                e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();

                e.HasOne(x => x.Sucursal)
                 .WithMany(s => s.Areas)
                 .HasForeignKey(x => x.SucursalID);
            });

            // ---------------- Consultorios ----------------
            modelBuilder.Entity<Consultorio>(e =>
            {
                e.ToTable("Consultorios");
                e.HasKey(x => x.ConsultorioID);
                e.Property(x => x.Codigo).HasColumnType("varchar(30)").IsRequired();
                e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
                e.Property(x => x.Piso).HasColumnType("varchar(20)");

                e.HasOne(x => x.Sucursal)
                 .WithMany()
                 .HasForeignKey(x => x.SucursalID);

                e.HasOne(x => x.Area)
                 .WithMany()
                 .HasForeignKey(x => x.AreaID);
            });

            // ---------------- Relaciones de Cita ----------------
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.EstadoCita)
                .WithMany(e => e.Citas)
                .HasForeignKey(c => c.EstadoCitaID);

            // La FK es opcional: una cita puede agendarse sin consultorio asignado.
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Consultorio)
                .WithMany(x => x.Citas)
                .HasForeignKey(c => c.ConsultorioID);
        }
    }
}
