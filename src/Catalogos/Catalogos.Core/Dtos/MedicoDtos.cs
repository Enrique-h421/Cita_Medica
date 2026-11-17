using System.ComponentModel.DataAnnotations;

namespace Catalogos.Core.Dtos
{
    public class MedicoDto
    {
        public int MedicoId { get; set; }
        public string CodigoMedico { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;

        /// <summary>Listo para pintar en un combo del frontend sin concatenar allá.</summary>
        public string NombreCompleto { get; set; } = string.Empty;

        public string? NumeroLicencia { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public bool Activo { get; set; }

        /// <summary>La marcada como Principal en Medico_Especialidad.</summary>
        public int? EspecialidadPrincipalId { get; set; }
        public string? EspecialidadPrincipalNombre { get; set; }

        /// <summary>Todas sus especialidades, para la ficha del médico.</summary>
        public List<string> Especialidades { get; set; } = new();
    }

    public class CrearMedicoDto
    {
        [Required(ErrorMessage = "El código del médico es obligatorio.")]
        [StringLength(50, ErrorMessage = "El código no puede pasar de 50 caracteres.")]
        public string CodigoMedico { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden pasar de 100 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden pasar de 100 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "El número de licencia no puede pasar de 100 caracteres.")]
        public string? NumeroLicencia { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string? Email { get; set; }

        public bool Activo { get; set; } = true;

        /// <summary>Especialidades a asignarle. Puede ir vacío.</summary>
        public List<int> EspecialidadesIds { get; set; } = new();

        /// <summary>Cuál de las anteriores queda marcada como principal.</summary>
        public int? EspecialidadPrincipalId { get; set; }
    }

    public class ActualizarMedicoDto
    {
        [Required(ErrorMessage = "El código del médico es obligatorio.")]
        [StringLength(50, ErrorMessage = "El código no puede pasar de 50 caracteres.")]
        public string CodigoMedico { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden pasar de 100 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden pasar de 100 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "El número de licencia no puede pasar de 100 caracteres.")]
        public string? NumeroLicencia { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string? Email { get; set; }

        public bool Activo { get; set; } = true;

        public List<int> EspecialidadesIds { get; set; } = new();

        public int? EspecialidadPrincipalId { get; set; }
    }
}
