using System;

namespace Core.Citas.Dtos
{
    public class CreateCitaDto
    {
        public long PacienteID { get; set; }
        public int MedicoID { get; set; }
        public int? EspecialidadID { get; set; }
        public int? ConsultorioID { get; set; }
        public int EstadoCitaID { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }
        public string? Motivo { get; set; }
        public string? Observaciones { get; set; }
        public int? UsuarioCreacionID { get; set; }

        // No incluye CitaID (lo genera la BD)
        // No incluye FechaCreacion (se asigna en el servidor)
    }
}