using Catalogos.Core.Dtos;
using Catalogos.Domain.Entities;

namespace Catalogos.Core.Mappings
{
    /// <summary>Traduce entre las entidades de Catálogos y sus DTOs.</summary>
    public static class CatalogosMapper
    {
        // ---------------- Especialidad ----------------

        public static EspecialidadDto ToDto(this Especialidad e) => new EspecialidadDto
        {
            EspecialidadId = e.EspecialidadID,
            Nombre = e.Nombre,
            Descripcion = e.Descripcion,
            Activo = e.Activo
        };

        public static List<EspecialidadDto> ToDtoList(this IEnumerable<Especialidad> items) =>
            items.Select(x => x.ToDto()).ToList();

        public static Especialidad ToEntity(this CrearEspecialidadDto dto) => new Especialidad
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activo = dto.Activo
        };

        public static void AplicarCambios(this ActualizarEspecialidadDto dto, Especialidad e)
        {
            e.Nombre = dto.Nombre;
            e.Descripcion = dto.Descripcion;
            e.Activo = dto.Activo;
        }

        // ---------------- Medico ----------------

        public static MedicoDto ToDto(this Medico m) => new MedicoDto
        {
            MedicoId = m.MedicoID,
            Nombres = m.Nombres,
            Apellidos = m.Apellidos,
            NombreCompleto = $"{m.Nombres} {m.Apellidos}".Trim(),
            NumeroJvpm = m.NumeroJVPM,
            EspecialidadId = m.EspecialidadID,
            EspecialidadNombre = m.Especialidad?.Nombre,
            Telefono = m.Telefono,
            Correo = m.Correo,
            Activo = m.Activo
        };

        public static List<MedicoDto> ToDtoList(this IEnumerable<Medico> items) =>
            items.Select(x => x.ToDto()).ToList();

        public static Medico ToEntity(this CrearMedicoDto dto) => new Medico
        {
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            NumeroJVPM = dto.NumeroJvpm,
            EspecialidadID = dto.EspecialidadId,
            Telefono = dto.Telefono,
            Correo = dto.Correo,
            Activo = dto.Activo
        };

        public static void AplicarCambios(this ActualizarMedicoDto dto, Medico m)
        {
            m.Nombres = dto.Nombres;
            m.Apellidos = dto.Apellidos;
            m.NumeroJVPM = dto.NumeroJvpm;
            m.EspecialidadID = dto.EspecialidadId;
            m.Telefono = dto.Telefono;
            m.Correo = dto.Correo;
            m.Activo = dto.Activo;
        }

        // ---------------- Paciente ----------------

        public static PacienteDto ToDto(this Paciente p) => new PacienteDto
        {
            PacienteId = p.PacienteID,
            Nombres = p.Nombres,
            Apellidos = p.Apellidos,
            NombreCompleto = $"{p.Nombres} {p.Apellidos}".Trim(),
            Dui = p.Dui,
            FechaNacimiento = p.FechaNacimiento,
            Edad = CalcularEdad(p.FechaNacimiento),
            Sexo = p.Sexo,
            Telefono = p.Telefono,
            Correo = p.Correo,
            Direccion = p.Direccion,
            Activo = p.Activo,
            FechaRegistro = p.FechaRegistro
        };

        public static List<PacienteDto> ToDtoList(this IEnumerable<Paciente> items) =>
            items.Select(x => x.ToDto()).ToList();

        public static Paciente ToEntity(this CrearPacienteDto dto) => new Paciente
        {
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Dui = dto.Dui,
            FechaNacimiento = dto.FechaNacimiento,
            Sexo = dto.Sexo,
            Telefono = dto.Telefono,
            Correo = dto.Correo,
            Direccion = dto.Direccion,
            Activo = dto.Activo,
            FechaRegistro = DateTime.Now
        };

        public static void AplicarCambios(this ActualizarPacienteDto dto, Paciente p)
        {
            p.Nombres = dto.Nombres;
            p.Apellidos = dto.Apellidos;
            p.Dui = dto.Dui;
            p.FechaNacimiento = dto.FechaNacimiento;
            p.Sexo = dto.Sexo;
            p.Telefono = dto.Telefono;
            p.Correo = dto.Correo;
            p.Direccion = dto.Direccion;
            p.Activo = dto.Activo;
        }

        private static int? CalcularEdad(DateTime? fechaNacimiento)
        {
            if (fechaNacimiento is null) return null;

            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Value.Year;

            // Si todavía no ha cumplido años este año, se le resta uno.
            if (fechaNacimiento.Value.Date > hoy.AddYears(-edad)) edad--;

            return edad < 0 ? null : edad;
        }
    }
}
