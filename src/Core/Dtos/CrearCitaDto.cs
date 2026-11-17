using System.ComponentModel.DataAnnotations;

namespace Core.Dtos
{
    /// <summary>
    /// Lo que el frontend manda para crear una cita. No trae CitaId ni FechaCreacion:
    /// esos los pone el servidor, no el cliente.
    /// </summary>
    public class CrearCitaDto
    {
        [Range(1, long.MaxValue, ErrorMessage = "Debe indicar un paciente válido.")]
        public long PacienteId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un médico válido.")]
        public int MedicoId { get; set; }

        public int? EspecialidadId { get; set; }

        public int? ConsultorioId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el estado de la cita.")]
        public int EstadoCitaId { get; set; }

        [Required(ErrorMessage = "La fecha y hora de inicio es obligatoria.")]
        public DateTime FechaHoraInicio { get; set; }

        public DateTime? FechaHoraFin { get; set; }

        [StringLength(500, ErrorMessage = "El motivo no puede pasar de 500 caracteres.")]
        public string? Motivo { get; set; }

        [StringLength(1000, ErrorMessage = "Las observaciones no pueden pasar de 1000 caracteres.")]
        public string? Observaciones { get; set; }
    }
}
