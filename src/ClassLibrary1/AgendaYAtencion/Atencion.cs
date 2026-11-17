using Domain.Expedientes;

namespace Domain.AgendaYAtencion
{
    /// <summary>
    /// La consulta en sí: el momento en que el médico recibe al paciente y
    /// registra lo que encontró. Una atención nace abierta, con su hora de
    /// inicio, y se cierra cuando el médico termina de documentarla.
    ///
    /// Puede venir de una cita agendada (CitaID) o ser espontánea, como una
    /// consulta de urgencia que nadie agendó. Por eso CitaID es opcional.
    /// </summary>
    public class Atencion
    {
        public long AtencionID { get; set; }

        /// <summary>Id del paciente en el microservicio de Catálogos.</summary>
        public long PacienteID { get; set; }

        /// <summary>Id del médico en el microservicio de Catálogos.</summary>
        public int MedicoID { get; set; }

        /// <summary>Cita que originó esta atención. Null si fue espontánea.</summary>
        public long? CitaID { get; set; }

        /// <summary>Expediente del paciente. Obligatorio: toda atención se archiva.</summary>
        public long ExpedienteID { get; set; }

        public DateTime FechaInicio { get; set; }

        /// <summary>Se llena al cerrar la atención. La base exige que sea posterior
        /// o igual a FechaInicio (constraint CK_Atenciones_Fechas).</summary>
        public DateTime? FechaFin { get; set; }

        public string? MotivoConsulta { get; set; }
        public string? EnfermedadActual { get; set; }
        public string? ExploracionFisica { get; set; }
        public string? Observaciones { get; set; }

        /// <summary>ABIERTA, CERRADA o ANULADA. Ver <see cref="EstadosAtencion"/>.</summary>
        public string Estado { get; set; } = EstadosAtencion.Abierta;

        public int? UsuarioCreacionID { get; set; }

        public Cita? Cita { get; set; }
        public ExpedienteClinico? Expediente { get; set; }
    }

    /// <summary>
    /// Estados por los que pasa una atención. La columna es varchar(30) libre,
    /// así que las constantes están aquí para que nadie escriba el texto a mano
    /// y se equivoque de mayúscula.
    /// </summary>
    public static class EstadosAtencion
    {
        /// <summary>El paciente está siendo atendido; todavía se puede documentar.</summary>
        public const string Abierta = "ABIERTA";

        /// <summary>El médico terminó y firmó. Queda en el expediente.</summary>
        public const string Cerrada = "CERRADA";

        /// <summary>Se registró por error. No se borra: en clínica nada se borra.</summary>
        public const string Anulada = "ANULADA";
    }
}
