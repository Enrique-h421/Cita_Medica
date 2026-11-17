namespace Catalogos.Domain.Entities
{
    /// <summary>
    /// Mapea dbo.Medicos del esquema entregado. Las especialidades no son una
    /// columna: van por la tabla puente Medico_Especialidad, porque un médico
    /// puede tener varias y una marcada como principal.
    /// </summary>
    public class Medico
    {
        public int MedicoID { get; set; }

        /// <summary>Enlace al usuario del sistema, si el médico tiene acceso.</summary>
        public int? UsuarioID { get; set; }

        public string CodigoMedico { get; set; } = string.Empty;

        public string Nombres { get; set; } = string.Empty;

        public string Apellidos { get; set; } = string.Empty;

        public string? NumeroLicencia { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<MedicoEspecialidad> Especialidades { get; set; } = new List<MedicoEspecialidad>();
    }
}
