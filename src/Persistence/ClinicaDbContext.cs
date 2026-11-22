using Domain.AgendaYAtencion;
using Domain.Diagnosticos;
using Domain.Expedientes;
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
        public DbSet<Atencion> Atenciones { get; set; }
        public DbSet<ExpedienteClinico> Expedientes_Clinicos { get; set; }
        public DbSet<Diagnostico> Diagnosticos { get; set; }
        public DbSet<AtencionDiagnostico> Atencion_Diagnostico { get; set; }

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

            // ---------------- Expedientes clínicos ----------------
            modelBuilder.Entity<ExpedienteClinico>(e =>
            {
                e.ToTable("Expedientes_Clinicos");
                e.HasKey(x => x.ExpedienteID);
                e.Property(x => x.NumeroExpediente).HasColumnType("varchar(50)").IsRequired();

                // En la base es date, no datetime2: sin esto EF manda la hora y
                // SQL Server la trunca sin avisar.
                e.Property(x => x.FechaApertura).HasColumnType("date");
            });

            // ---------------- Atenciones ----------------
            modelBuilder.Entity<Atencion>(e =>
            {
                e.ToTable("Atenciones");
                e.HasKey(x => x.AtencionID);
                e.Property(x => x.MotivoConsulta).HasMaxLength(2000);
                e.Property(x => x.Estado).HasColumnType("varchar(30)").IsRequired();

                // EnfermedadActual, ExploracionFisica y Observaciones son
                // nvarchar(max) en la base, que es justo lo que EF asume por
                // defecto para un string sin largo declarado.

                e.HasOne(x => x.Expediente)
                 .WithMany(x => x.Atenciones)
                 .HasForeignKey(x => x.ExpedienteID);

                // Opcional: una urgencia se atiende sin cita previa.
                e.HasOne(x => x.Cita)
                 .WithMany()
                 .HasForeignKey(x => x.CitaID);
            });

            // ---------------- Catálogo de diagnósticos (CIE-10) ----------------
            modelBuilder.Entity<Diagnostico>(e =>
            {
                e.ToTable("Diagnosticos");
                e.HasKey(x => x.DiagnosticoID);

                // En la base es varchar(20), no nvarchar: un código CIE-10 es
                // ASCII puro ("J02.9"), así que no hace falta Unicode.
                e.Property(x => x.CodigoCIE10).HasColumnType("varchar(20)");

                e.Property(x => x.Nombre).HasMaxLength(300).IsRequired();
                e.Property(x => x.Descripcion).HasMaxLength(1000);
            });

            // ---------------- Diagnósticos de una atención ----------------
            modelBuilder.Entity<AtencionDiagnostico>(e =>
            {
                e.ToTable("Atencion_Diagnostico");
                e.HasKey(x => x.AtencionDiagnosticoID);
                e.Property(x => x.TipoDiagnostico).HasColumnType("varchar(30)").IsRequired();
                e.Property(x => x.Observaciones).HasMaxLength(1000);

                // Sin colección inversa en Atencion a propósito: los
                // diagnósticos se cargan desde esta tabla hacia el catálogo,
                // que es un solo nivel de include. Cargarlos desde la atención
                // necesitaría ThenInclude, que GenericPersistence no soporta.
                e.HasOne(x => x.Atencion)
                 .WithMany()
                 .HasForeignKey(x => x.AtencionID);

                e.HasOne(x => x.Diagnostico)
                 .WithMany()
                 .HasForeignKey(x => x.DiagnosticoID);
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
