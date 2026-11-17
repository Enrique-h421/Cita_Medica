using System.ComponentModel.DataAnnotations;

namespace Catalogos.Core.Dtos
{
    public class MedicoDto
    {
        public int MedicoId { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;

        /// <summary>Listo para pintar en un combo del frontend sin concatenar allá.</summary>
        public string NombreCompleto { get; set; } = string.Empty;

        public string? NumeroJvpm { get; set; }
        public int? EspecialidadId { get; set; }
        public string? EspecialidadNombre { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public bool Activo { get; set; }
    }

    public class CrearMedicoDto
    {
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden pasar de 100 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden pasar de 100 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "El número JVPM no puede pasar de 30 caracteres.")]
        public string? NumeroJvpm { get; set; }

        public int? EspecialidadId { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string? Correo { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class ActualizarMedicoDto
    {
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden pasar de 100 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden pasar de 100 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "El número JVPM no puede pasar de 30 caracteres.")]
        public string? NumeroJvpm { get; set; }

        public int? EspecialidadId { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string? Correo { get; set; }

        public bool Activo { get; set; } = true;
    }
}
