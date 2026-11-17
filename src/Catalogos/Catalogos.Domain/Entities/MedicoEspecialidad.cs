namespace Catalogos.Domain.Entities
{
    /// <summary>
    /// Tabla puente dbo.Medico_Especialidad. Llave compuesta (MedicoID,
    /// EspecialidadID) y una bandera que marca cuál es la especialidad
    /// principal del médico, que es la que se muestra en listados.
    /// </summary>
    public class MedicoEspecialidad
    {
        public int MedicoID { get; set; }

        public int EspecialidadID { get; set; }

        public bool Principal { get; set; }

        public Medico? Medico { get; set; }

        public Especialidad? Especialidad { get; set; }
    }
}
