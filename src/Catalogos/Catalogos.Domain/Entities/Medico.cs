namespace Catalogos.Domain.Entities
{
    /// <summary>Médico que atiende citas.</summary>
    public class Medico
    {
        public int MedicoID { get; set; }

        public string Nombres { get; set; } = string.Empty;

        public string Apellidos { get; set; } = string.Empty;

        /// <summary>Número de la Junta de Vigilancia de la Profesión Médica.</summary>
        public string? NumeroJVPM { get; set; }

        public int? EspecialidadID { get; set; }

        public string? Telefono { get; set; }

        public string? Correo { get; set; }

        public bool Activo { get; set; } = true;

        public Especialidad? Especialidad { get; set; }
    }
}
