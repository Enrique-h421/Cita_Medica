namespace Core.Dtos
{
    /// <summary>
    /// El expediente clínico visto desde la API. Es de solo lectura a propósito:
    /// no hay endpoint para crearlo ni para borrarlo, porque el expediente nace
    /// solo en la primera atención del paciente y en una clínica no se elimina.
    /// </summary>
    public class ExpedienteDto
    {
        public long ExpedienteId { get; set; }

        public long PacienteId { get; set; }
        public string? PacienteNombre { get; set; }

        public string NumeroExpediente { get; set; } = string.Empty;

        public DateTime FechaApertura { get; set; }

        public string? Observaciones { get; set; }

        public bool Activo { get; set; }

        /// <summary>Cuántas atenciones lleva registradas este expediente.</summary>
        public int TotalAtenciones { get; set; }
    }
}
