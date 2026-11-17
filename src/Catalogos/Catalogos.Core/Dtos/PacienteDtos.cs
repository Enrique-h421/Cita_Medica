using System.ComponentModel.DataAnnotations;

namespace Catalogos.Core.Dtos
{
    public class PacienteDto
    {
        public long PacienteId { get; set; }
        public string CodigoPaciente { get; set; } = string.Empty;

        public string TipoDocumento { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;

        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;

        /// <summary>Listo para pintar en un combo del frontend sin concatenar allá.</summary>
        public string NombreCompleto { get; set; } = string.Empty;

        public DateTime FechaNacimiento { get; set; }

        /// <summary>Calculada al vuelo, no se guarda en la base.</summary>
        public int? Edad { get; set; }

        public string Sexo { get; set; } = string.Empty;
        public string? EstadoCivil { get; set; }
        public string? Telefono { get; set; }
        public string? TelefonoSecundario { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Pais { get; set; }
        public string? Ocupacion { get; set; }
        public string? TipoSangre { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class CrearPacienteDto
    {
        [Required(ErrorMessage = "El código del paciente es obligatorio.")]
        [StringLength(50, ErrorMessage = "El código no puede pasar de 50 caracteres.")]
        public string CodigoPaciente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
        [StringLength(30)]
        public string TipoDocumento { get; set; } = "DUI";

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [StringLength(50)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El sexo es obligatorio.")]
        [StringLength(1, ErrorMessage = "El sexo se guarda como M o F.")]
        public string Sexo { get; set; } = string.Empty;

        [StringLength(30)]
        public string? EstadoCivil { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [StringLength(30)]
        public string? TelefonoSecundario { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? Ciudad { get; set; }

        [StringLength(100)]
        public string? Pais { get; set; }

        [StringLength(150)]
        public string? Ocupacion { get; set; }

        [StringLength(10)]
        public string? TipoSangre { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class ActualizarPacienteDto
    {
        [Required(ErrorMessage = "El código del paciente es obligatorio.")]
        [StringLength(50, ErrorMessage = "El código no puede pasar de 50 caracteres.")]
        public string CodigoPaciente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
        [StringLength(30)]
        public string TipoDocumento { get; set; } = "DUI";

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [StringLength(50)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El sexo es obligatorio.")]
        [StringLength(1, ErrorMessage = "El sexo se guarda como M o F.")]
        public string Sexo { get; set; } = string.Empty;

        [StringLength(30)]
        public string? EstadoCivil { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [StringLength(30)]
        public string? TelefonoSecundario { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? Ciudad { get; set; }

        [StringLength(100)]
        public string? Pais { get; set; }

        [StringLength(150)]
        public string? Ocupacion { get; set; }

        [StringLength(10)]
        public string? TipoSangre { get; set; }

        public bool Activo { get; set; } = true;
    }
}
