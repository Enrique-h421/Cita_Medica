namespace Catalogos.Domain.Entities
{
    /// <summary>Mapea dbo.Especialidades del esquema entregado.</summary>
    public class Especialidad
    {
        public int EspecialidadID { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<MedicoEspecialidad> Medicos { get; set; } = new List<MedicoEspecialidad>();
    }
}
