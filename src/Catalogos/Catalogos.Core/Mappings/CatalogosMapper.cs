using Catalogos.Core.Dtos;
using Catalogos.Domain.Entities;

namespace Catalogos.Core.Mappings
{
    /// <summary>
    /// Traduce entre las entidades de Catálogos y sus DTOs.
    /// Las especialidades del médico NO se llenan aquí: viven en una tabla
    /// puente y las resuelve el handler, porque el repositorio genérico no
    /// soporta includes anidados.
    /// </summary>
    public static class CatalogosMapper
    {
        // ---------------- Especialidad ----------------

        public static EspecialidadDto ToDto(this Especialidad e) => new EspecialidadDto
        {
            EspecialidadId = e.EspecialidadID,
            Codigo = e.Codigo,
            Nombre = e.Nombre,
            Descripcion = e.Descripcion,
            Activo = e.Activo
        };

        public static List<EspecialidadDto> ToDtoList(this IEnumerable<Especialidad> items) =>
            items.Select(x => x.ToDto()).ToList();

        public static Especialidad ToEntity(this CrearEspecialidadDto dto) => new Especialidad
        {
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activo = dto.Activo
        };

        public static void AplicarCambios(this ActualizarEspecialidadDto dto, Especialidad e)
        {
            e.Codigo = dto.Codigo;
            e.Nombre = dto.Nombre;
            e.Descripcion = dto.Descripcion;
            e.Activo = dto.Activo;
        }

        // ---------------- Medico ----------------

        public static MedicoDto ToDto(this Medico m) => new MedicoDto
        {
            MedicoId = m.MedicoID,
            CodigoMedico = m.CodigoMedico,
            Nombres = m.Nombres,
            Apellidos = m.Apellidos,
            NombreCompleto = $"{m.Nombres} {m.Apellidos}".Trim(),
            NumeroLicencia = m.NumeroLicencia,
            Telefono = m.Telefono,
            Email = m.Email,
            Activo = m.Activo
        };

        public static List<MedicoDto> ToDtoList(this IEnumerable<Medico> items) =>
            items.Select(x => x.ToDto()).ToList();

        public static Medico ToEntity(this CrearMedicoDto dto) => new Medico
        {
            CodigoMedico = dto.CodigoMedico,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            NumeroLicencia = dto.NumeroLicencia,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Activo = dto.Activo
        };

        public static void AplicarCambios(this ActualizarMedicoDto dto, Medico m)
        {
            m.CodigoMedico = dto.CodigoMedico;
            m.Nombres = dto.Nombres;
            m.Apellidos = dto.Apellidos;
            m.NumeroLicencia = dto.NumeroLicencia;
            m.Telefono = dto.Telefono;
            m.Email = dto.Email;
            m.Activo = dto.Activo;
        }

        // ---------------- Paciente ----------------

        public static PacienteDto ToDto(this Paciente p) => new PacienteDto
        {
            PacienteId = p.PacienteID,
            CodigoPaciente = p.CodigoPaciente,
            TipoDocumento = p.TipoDocumento,
            NumeroDocumento = p.NumeroDocumento,
            Nombres = p.Nombres,
            Apellidos = p.Apellidos,
            NombreCompleto = $"{p.Nombres} {p.Apellidos}".Trim(),
            FechaNacimiento = p.FechaNacimiento,
            Edad = CalcularEdad(p.FechaNacimiento),
            Sexo = p.Sexo,
            EstadoCivil = p.EstadoCivil,
            Telefono = p.Telefono,
            TelefonoSecundario = p.TelefonoSecundario,
            Email = p.Email,
            Direccion = p.Direccion,
            Ciudad = p.Ciudad,
            Pais = p.Pais,
            Ocupacion = p.Ocupacion,
            TipoSangre = p.TipoSangre,
            Activo = p.Activo,
            FechaRegistro = p.FechaRegistro
        };

        public static List<PacienteDto> ToDtoList(this IEnumerable<Paciente> items) =>
            items.Select(x => x.ToDto()).ToList();

        public static Paciente ToEntity(this CrearPacienteDto dto) => new Paciente
        {
            CodigoPaciente = dto.CodigoPaciente,
            TipoDocumento = dto.TipoDocumento,
            NumeroDocumento = dto.NumeroDocumento,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            FechaNacimiento = dto.FechaNacimiento,
            Sexo = dto.Sexo,
            EstadoCivil = dto.EstadoCivil,
            Telefono = dto.Telefono,
            TelefonoSecundario = dto.TelefonoSecundario,
            Email = dto.Email,
            Direccion = dto.Direccion,
            Ciudad = dto.Ciudad,
            Pais = dto.Pais,
            Ocupacion = dto.Ocupacion,
            TipoSangre = dto.TipoSangre,
            Activo = dto.Activo,
            FechaRegistro = DateTime.Now
        };

        public static void AplicarCambios(this ActualizarPacienteDto dto, Paciente p)
        {
            p.CodigoPaciente = dto.CodigoPaciente;
            p.TipoDocumento = dto.TipoDocumento;
            p.NumeroDocumento = dto.NumeroDocumento;
            p.Nombres = dto.Nombres;
            p.Apellidos = dto.Apellidos;
            p.FechaNacimiento = dto.FechaNacimiento;
            p.Sexo = dto.Sexo;
            p.EstadoCivil = dto.EstadoCivil;
            p.Telefono = dto.Telefono;
            p.TelefonoSecundario = dto.TelefonoSecundario;
            p.Email = dto.Email;
            p.Direccion = dto.Direccion;
            p.Ciudad = dto.Ciudad;
            p.Pais = dto.Pais;
            p.Ocupacion = dto.Ocupacion;
            p.TipoSangre = dto.TipoSangre;
            p.Activo = dto.Activo;
        }

        private static int? CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;

            // Si todavía no ha cumplido años este año, se le resta uno.
            if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;

            return edad < 0 ? null : edad;
        }
    }
}
