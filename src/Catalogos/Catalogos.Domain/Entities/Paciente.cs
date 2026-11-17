namespace Catalogos.Domain.Entities
{
    /// <summary>Paciente registrado en la clínica.</summary>
    public class Paciente
    {
        public long PacienteID { get; set; }

        public string Nombres { get; set; } = string.Empty;

        public string Apellidos { get; set; } = string.Empty;

        public string? Dui { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        /// <summary>M o F.</summary>
        public string? Sexo { get; set; }

        public string? Telefono { get; set; }

        public string? Correo { get; set; }

        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; }
    }
}
