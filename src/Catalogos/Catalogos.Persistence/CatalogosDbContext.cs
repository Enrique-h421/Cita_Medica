using Catalogos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalogos.Persistence
{
    /// <summary>
    /// Mapea las tablas de catálogo del esquema entregado en ClinicaDB.
    ///
    /// Los dos microservicios comparten base de datos porque el esquema del
    /// ingeniero es un único diseño relacional con llaves foráneas cruzadas
    /// (Expedientes y Atenciones exigen Pacientes). La separación de contextos
    /// está en el código: cada microservicio declara solo sus tablas, tiene su
    /// propio DbContext y su propia API, y se hablan por REST.
    ///
    /// Los tipos se declaran explícitos (varchar vs nvarchar) porque las tablas
    /// ya existen y EF debe generar SQL que les calce.
    /// </summary>
    public class CatalogosDbContext : DbContext
    {
        public CatalogosDbContext(DbContextOptions<CatalogosDbContext> options) : base(options) { }

        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Medico> Medicos { get; set; }
        public DbSet<MedicoEspecialidad> MedicoEspecialidades { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------------- Especialidades ----------------
            modelBuilder.Entity<Especialidad>(e =>
            {
                e.ToTable("Especialidades");
                e.HasKey(x => x.EspecialidadID);
                e.Property(x => x.Codigo).HasColumnType("varchar(30)").IsRequired();
                e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
                e.Property(x => x.Descripcion).HasMaxLength(300);
            });

            // ---------------- Medicos ----------------
            modelBuilder.Entity<Medico>(e =>
            {
                e.ToTable("Medicos");
                e.HasKey(x => x.MedicoID);
                e.Property(x => x.CodigoMedico).HasColumnType("varchar(50)").IsRequired();
                e.Property(x => x.Nombres).HasMaxLength(100).IsRequired();
                e.Property(x => x.Apellidos).HasMaxLength(100).IsRequired();
                e.Property(x => x.NumeroLicencia).HasColumnType("varchar(100)");
                e.Property(x => x.Telefono).HasColumnType("varchar(30)");
                e.Property(x => x.Email).HasColumnType("varchar(150)");
            });

            // ---------------- Medico_Especialidad (puente) ----------------
            modelBuilder.Entity<MedicoEspecialidad>(e =>
            {
                e.ToTable("Medico_Especialidad");
                e.HasKey(x => new { x.MedicoID, x.EspecialidadID });

                e.HasOne(x => x.Medico)
                 .WithMany(m => m.Especialidades)
                 .HasForeignKey(x => x.MedicoID);

                e.HasOne(x => x.Especialidad)
                 .WithMany(s => s.Medicos)
                 .HasForeignKey(x => x.EspecialidadID);
            });

            // ---------------- Pacientes ----------------
            modelBuilder.Entity<Paciente>(e =>
            {
                e.ToTable("Pacientes");
                e.HasKey(x => x.PacienteID);
                e.Property(x => x.CodigoPaciente).HasColumnType("varchar(50)").IsRequired();
                e.Property(x => x.TipoDocumento).HasColumnType("varchar(30)").IsRequired();
                e.Property(x => x.NumeroDocumento).HasColumnType("varchar(50)").IsRequired();
                e.Property(x => x.Nombres).HasMaxLength(100).IsRequired();
                e.Property(x => x.Apellidos).HasMaxLength(100).IsRequired();
                e.Property(x => x.FechaNacimiento).HasColumnType("date");
                e.Property(x => x.Sexo).HasColumnType("char(1)").IsRequired();
                e.Property(x => x.EstadoCivil).HasColumnType("varchar(30)");
                e.Property(x => x.Telefono).HasColumnType("varchar(30)");
                e.Property(x => x.TelefonoSecundario).HasColumnType("varchar(30)");
                e.Property(x => x.Email).HasColumnType("varchar(150)");
                e.Property(x => x.Direccion).HasMaxLength(300);
                e.Property(x => x.Ciudad).HasMaxLength(100);
                e.Property(x => x.Pais).HasMaxLength(100);
                e.Property(x => x.Ocupacion).HasMaxLength(150);
                e.Property(x => x.TipoSangre).HasColumnType("varchar(10)");
            });
        }
    }
}
