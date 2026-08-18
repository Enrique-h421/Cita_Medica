using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.AgendaYAtencion
{
    public class Cita
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
        public int? UsuarioCreacionID { get; set; }
        public DateTime FechaCreacion { get; set; }

        
        public EstadoCita? EstadoCita { get; set; }
    }
}