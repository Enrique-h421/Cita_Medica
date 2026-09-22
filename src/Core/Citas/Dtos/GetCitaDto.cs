using System;

namespace Core.Citas.Dtos
{
    public class GetCitaDto
    {
        public long CitaID { get; set; }
        public long PacienteID { get; set; }
        public int MedicoID { get; set; }
        public int? EspecialidadID { get; set; }
        public int? ConsultorioID { get; set; }
        public int EstadoCitaID { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }
        public string? Motivo { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Ojo: ajusta este campo al nombre real de la propiedad
        // descriptiva que tenga tu clase EstadoCita (ej. "Descripcion" o "Nombre")
        public string? EstadoCitaDescripcion { get; set; }
    }
}