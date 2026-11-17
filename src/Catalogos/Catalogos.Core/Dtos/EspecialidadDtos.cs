using System.ComponentModel.DataAnnotations;

namespace Catalogos.Core.Dtos
{
    public class EspecialidadDto
    {
        public int EspecialidadId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }

    public class CrearEspecialidadDto
    {
        [Required(ErrorMessage = "El código de la especialidad es obligatorio.")]
        [StringLength(30, ErrorMessage = "El código no puede pasar de 30 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la especialidad es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede pasar de 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "La descripción no puede pasar de 300 caracteres.")]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class ActualizarEspecialidadDto
    {
        [Required(ErrorMessage = "El código de la especialidad es obligatorio.")]
        [StringLength(30, ErrorMessage = "El código no puede pasar de 30 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la especialidad es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede pasar de 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "La descripción no puede pasar de 300 caracteres.")]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
