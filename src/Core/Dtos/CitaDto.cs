namespace Core.Dtos
{
    /// <summary>
    /// Lo que la API devuelve al frontend. Es plano a propósito: el estado viaja
    /// como código y nombre, no como el objeto EstadoCita completo, para que la
    /// respuesta no arrastre de vuelta todas las demás citas de ese estado.
    ///
    /// El nombre del consultorio sale de la propia base de Agenda. Los de paciente,
    /// médico y especialidad no: se resuelven consultando el microservicio de
    /// Catálogos. Si ese servicio no responde llegan en null y la cita se muestra igual.
    /// </summary>
    public class CitaDto
    {
        public long CitaId { get; set; }

        public long PacienteId { get; set; }
        public string? PacienteNombre { get; set; }

        public int MedicoId { get; set; }
        public string? MedicoNombre { get; set; }

        public int? EspecialidadId { get; set; }
        public string? EspecialidadNombre { get; set; }

        public int? ConsultorioId { get; set; }
        public string? ConsultorioNombre { get; set; }

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
