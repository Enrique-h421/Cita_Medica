using Catalogos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalogos.Persistence
{
    /// <summary>
    /// Base de datos propia del microservicio de Catálogos. No comparte tablas
    /// con Agenda: cada microservicio es dueño de sus datos.
    /// </summary>
    public class CatalogosDbContext : DbContext
    {
        public CatalogosDbContext(DbContextOptions<CatalogosDbContext> options) : base(options) { }

        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Medico> Medicos { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------------- Especialidades ----------------
            modelBuilder.Entity<Especialidad>(e =>
            {
                e.ToTable("Especialidades");
                e.HasKey(x => x.EspecialidadID);
                e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
                e.Property(x => x.Descripcion).HasMaxLength(300);
            });

            // ---------------- Medicos ----------------
            modelBuilder.Entity<Medico>(e =>
            {
                e.ToTable("Medicos");
                e.HasKey(x => x.MedicoID);
                e.Property(x => x.Nombres).HasMaxLength(100).IsRequired();
                e.Property(x => x.Apellidos).HasMaxLength(100).IsRequired();
                e.Property(x => x.NumeroJVPM).HasMaxLength(30);
                e.Property(x => x.Telefono).HasMaxLength(30);
                e.Property(x => x.Correo).HasMaxLength(150);

                e.HasOne(x => x.Especialidad)
                 .WithMany(x => x.Medicos)
                 .HasForeignKey(x => x.EspecialidadID)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ---------------- Pacientes ----------------
            modelBuilder.Entity<Paciente>(e =>
            {
                e.ToTable("Pacientes");
                e.HasKey(x => x.PacienteID);
                e.Property(x => x.Nombres).HasMaxLength(100).IsRequired();
                e.Property(x => x.Apellidos).HasMaxLength(100).IsRequired();
                e.Property(x => x.Dui).HasMaxLength(15);
                e.Property(x => x.Sexo).HasMaxLength(1);
                e.Property(x => x.Telefono).HasMaxLength(30);
                e.Property(x => x.Correo).HasMaxLength(150);
                e.Property(x => x.Direccion).HasMaxLength(300);
            });

            SembrarDatos(modelBuilder);
        }

        /// <summary>
        /// Datos de arranque. Viajan dentro de la migración, así que al correr
        /// Update-Database la base queda creada y poblada de una vez.
        /// Los Id 1 de médico y paciente existen a propósito: las citas que ya
        /// están en ClinicaDB apuntan a ellos.
        /// </summary>
        private static void SembrarDatos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Especialidad>().HasData(
                new Especialidad { EspecialidadID = 1, Nombre = "Medicina General", Descripcion = "Consulta general y control de rutina", Activo = true },
                new Especialidad { EspecialidadID = 2, Nombre = "Pediatría", Descripcion = "Atención de niños y adolescentes", Activo = true },
                new Especialidad { EspecialidadID = 3, Nombre = "Ginecología", Descripcion = "Salud de la mujer", Activo = true },
                new Especialidad { EspecialidadID = 4, Nombre = "Cardiología", Descripcion = "Corazón y sistema circulatorio", Activo = true },
                new Especialidad { EspecialidadID = 5, Nombre = "Dermatología", Descripcion = "Piel, cabello y uñas", Activo = true }
            );

            modelBuilder.Entity<Medico>().HasData(
                new Medico { MedicoID = 1, Nombres = "Carlos Alberto", Apellidos = "Menjívar Rivas", NumeroJVPM = "JVPM-10234", EspecialidadID = 1, Telefono = "2250-1100", Correo = "cmenjivar@clinica.sv", Activo = true },
                new Medico { MedicoID = 2, Nombres = "Ana Beatriz", Apellidos = "Portillo Guzmán", NumeroJVPM = "JVPM-10987", EspecialidadID = 2, Telefono = "2250-1101", Correo = "aportillo@clinica.sv", Activo = true },
                new Medico { MedicoID = 3, Nombres = "José Ricardo", Apellidos = "Hernández Mejía", NumeroJVPM = "JVPM-11455", EspecialidadID = 4, Telefono = "2250-1102", Correo = "jhernandez@clinica.sv", Activo = true },
                new Medico { MedicoID = 4, Nombres = "Sofía Elena", Apellidos = "Cruz Alvarado", NumeroJVPM = "JVPM-11890", EspecialidadID = 3, Telefono = "2250-1103", Correo = "scruz@clinica.sv", Activo = true }
            );

            var registro = new DateTime(2026, 1, 15, 8, 0, 0);

            modelBuilder.Entity<Paciente>().HasData(
                new Paciente { PacienteID = 1, Nombres = "María José", Apellidos = "Argueta Flores", Dui = "04567891-2", FechaNacimiento = new DateTime(1992, 3, 14), Sexo = "F", Telefono = "7845-2210", Correo = "mjargueta@correo.com", Direccion = "Col. Escalón, San Salvador", Activo = true, FechaRegistro = registro },
                new Paciente { PacienteID = 2, Nombres = "Juan Carlos", Apellidos = "Martínez Lemus", Dui = "03218765-4", FechaNacimiento = new DateTime(1985, 11, 2), Sexo = "M", Telefono = "7712-9087", Correo = "jcmartinez@correo.com", Direccion = "Soyapango, San Salvador", Activo = true, FechaRegistro = registro },
                new Paciente { PacienteID = 3, Nombres = "Rosa Elena", Apellidos = "Flores Campos", Dui = "05543210-9", FechaNacimiento = new DateTime(1978, 7, 25), Sexo = "F", Telefono = "7033-4512", Correo = "reflores@correo.com", Direccion = "Santa Tecla, La Libertad", Activo = true, FechaRegistro = registro },
                new Paciente { PacienteID = 4, Nombres = "Luis Alberto", Apellidos = "Ramírez Peña", Dui = "02987654-1", FechaNacimiento = new DateTime(2001, 1, 30), Sexo = "M", Telefono = "7890-1123", Correo = "laramirez@correo.com", Direccion = "Mejicanos, San Salvador", Activo = true, FechaRegistro = registro },
                new Paciente { PacienteID = 5, Nombres = "Carmen Lidia", Apellidos = "Sánchez Mendoza", Dui = "06112233-5", FechaNacimiento = new DateTime(1966, 9, 8), Sexo = "F", Telefono = "7456-7788", Correo = "clsanchez@correo.com", Direccion = "San Miguel", Activo = true, FechaRegistro = registro },
                new Paciente { PacienteID = 6, Nombres = "Óscar Mauricio", Apellidos = "Rivera Call", Dui = "07334455-6", FechaNacimiento = new DateTime(1995, 5, 19), Sexo = "M", Telefono = "6023-9900", Correo = "omrivera@correo.com", Direccion = "Santa Ana", Activo = true, FechaRegistro = registro }
            );
        }
    }
}
