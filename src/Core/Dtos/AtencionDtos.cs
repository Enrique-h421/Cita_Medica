using System.ComponentModel.DataAnnotations;

namespace Core.Dtos
{
    /// <summary>
    /// Lo que la API devuelve de una atención. Es plano: el número de expediente
    /// viene resuelto y los nombres de paciente y médico se consultan al
    /// microservicio de Catálogos. Si Catálogos no responde llegan en null y la
    /// atención se muestra igual.
    /// </summary>
    public class AtencionDto
    {
        public long AtencionId { get; set; }

        public long PacienteId { get; set; }
        public string? PacienteNombre { get; set; }

        public int MedicoId { get; set; }
        public string? MedicoNombre { get; set; }

        /// <summary>Cita que la originó. Null si fue una consulta espontánea.</summary>
        public long? CitaId { get; set; }

        public long ExpedienteId { get; set; }
        public string? NumeroExpediente { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        /// <summary>Cuánto duró la consulta. Null mientras siga abierta.</summary>
        public int? DuracionMinutos { get; set; }

        public string? MotivoConsulta { get; set; }
        public string? EnfermedadActual { get; set; }
        public string? ExploracionFisica { get; set; }
        public string? Observaciones { get; set; }

        /// <summary>ABIERTA, CERRADA o ANULADA.</summary>
        public string Estado { get; set; } = string.Empty;
    }

    /// <summary>
    /// Datos para abrir una atención. No se pide el expediente: si el paciente
    /// no tiene uno, el sistema se lo abre solo.
    /// </summary>
    public class CrearAtencionDto
    {
        [Range(1, long.MaxValue, ErrorMessage = "Debe indicar el paciente que se está atendiendo.")]
        public long PacienteId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el médico que atiende.")]
        public int MedicoId { get; set; }

        /// <summary>Cita agendada que se está atendiendo. Se deja vacío en urgencias.</summary>
        public long? CitaId { get; set; }

        /// <summary>Si no se manda, se toma la hora del servidor.</summary>
        public DateTime? FechaInicio { get; set; }

        [StringLength(2000, ErrorMessage = "El motivo de consulta no puede pasar de 2000 caracteres.")]
        public string? MotivoConsulta { get; set; }

        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Los hallazgos con los que el médico cierra la consulta. Al cerrarla, la
    /// cita que la originó pasa a "Atendida".
    /// </summary>
    public class CerrarAtencionDto
    {
        /// <summary>Si no se manda, se toma la hora del servidor.</summary>
        public DateTime? FechaFin { get; set; }

        [StringLength(2000, ErrorMessage = "El motivo de consulta no puede pasar de 2000 caracteres.")]
        public string? MotivoConsulta { get; set; }

        public string? EnfermedadActual { get; set; }
        public string? ExploracionFisica { get; set; }
        public string? Observaciones { get; set; }
    }

    /// <summary>Corrige lo documentado en una atención que sigue abierta.</summary>
    public class ActualizarAtencionDto
    {
        [StringLength(2000, ErrorMessage = "El motivo de consulta no puede pasar de 2000 caracteres.")]
        public string? MotivoConsulta { get; set; }

        public string? EnfermedadActual { get; set; }
        public string? ExploracionFisica { get; set; }
        public string? Observaciones { get; set; }
    }
}
