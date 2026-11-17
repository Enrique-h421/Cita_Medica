namespace Catalogos.Domain.Entities
{
    /// <summary>Especialidad médica a la que pertenece un médico y a la que
    /// se puede dirigir una cita.</summary>
    public class Especialidad
    {
        public int EspecialidadID { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Medico> Medicos { get; set; } = new List<Medico>();
    }
}
