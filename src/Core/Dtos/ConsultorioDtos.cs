using System.ComponentModel.DataAnnotations;

namespace Core.Dtos
{
    public class ConsultorioDto
    {
        public int ConsultorioId { get; set; }

        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Piso { get; set; }

        public int SucursalId { get; set; }
        public string? SucursalNombre { get; set; }

        public int? AreaId { get; set; }
        public string? AreaNombre { get; set; }

        public bool Activo { get; set; }

        /// <summary>Listo para pintar en un combo: "C-101 — Consultorio 1 (Consulta Externa)".</summary>
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CrearConsultorioDto
    {
        [Required(ErrorMessage = "El código del consultorio es obligatorio.")]
        [StringLength(30, ErrorMessage = "El código no puede pasar de 30 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del consultorio es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El piso no puede pasar de 20 caracteres.")]
        public string? Piso { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la sucursal a la que pertenece.")]
        public int SucursalId { get; set; }

        public int? AreaId { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class ActualizarConsultorioDto
    {
        [Required(ErrorMessage = "El código del consultorio es obligatorio.")]
        [StringLength(30, ErrorMessage = "El código no puede pasar de 30 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del consultorio es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El piso no puede pasar de 20 caracteres.")]
        public string? Piso { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la sucursal a la que pertenece.")]
        public int SucursalId { get; set; }

        public int? AreaId { get; set; }

        public bool Activo { get; set; } = true;
    }
}
