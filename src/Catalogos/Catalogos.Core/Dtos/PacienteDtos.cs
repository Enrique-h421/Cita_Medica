using System.ComponentModel.DataAnnotations;

namespace Catalogos.Core.Dtos
{
    public class PacienteDto
    {
        public long PacienteId { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;

        /// <summary>Listo para pintar en un combo del frontend sin concatenar allá.</summary>
        public string NombreCompleto { get; set; } = string.Empty;

        public string? Dui { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        /// <summary>Calculada al vuelo, no se guarda en la base.</summary>
        public int? Edad { get; set; }

        public string? Sexo { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class CrearPacienteDto
    {
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden pasar de 100 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden pasar de 100 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "El DUI no puede pasar de 15 caracteres.")]
        public string? Dui { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(1, ErrorMessage = "El sexo se guarda como M o F.")]
        public string? Sexo { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string? Correo { get; set; }

        [StringLength(300)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class ActualizarPacienteDto
    {
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden pasar de 100 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden pasar de 100 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "El DUI no puede pasar de 15 caracteres.")]
        public string? Dui { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(1, ErrorMessage = "El sexo se guarda como M o F.")]
        public string? Sexo { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string? Correo { get; set; }

        [StringLength(300)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
