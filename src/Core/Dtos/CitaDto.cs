namespace Core.Dtos
{
    /// <summary>
    /// Lo que la API devuelve al frontend. Es plano a propósito: el estado viaja
    /// como código y nombre, no como el objeto EstadoCita completo, para que la
    /// respuesta no arrastre de vuelta todas las demás citas de ese estado.
    /// </summary>
    public class CitaDto
    {
        public long CitaId { get; set; }

        public long PacienteId { get; set; }
        public int MedicoId { get; set; }
        public int? EspecialidadId { get; set; }
        public int? ConsultorioId { get; set; }

        public int EstadoCitaId { get; set; }
        public string? EstadoCodigo { get; set; }
        public string? EstadoNombre { get; set; }

        public DateTime FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }

        public string? Motivo { get; set; }
        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
