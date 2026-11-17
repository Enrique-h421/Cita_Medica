using Core.Dtos;
using Domain.AgendaYAtencion;

namespace Core.Mappings
{
    /// <summary>
    /// Traduce entre la entidad de dominio Cita y los DTOs que viajan por la API.
    /// Se hace a mano a propósito: son pocos campos y así no se mete otra
    /// dependencia al proyecto solo para copiar propiedades.
    /// </summary>
    public static class CitaMapper
    {
        public static CitaDto ToDto(this Cita cita) => new CitaDto
        {
            CitaId = cita.CitaID,
            PacienteId = cita.PacienteID,
            MedicoId = cita.MedicoID,
            EspecialidadId = cita.EspecialidadID,
            ConsultorioId = cita.ConsultorioID,

            EstadoCitaId = cita.EstadoCitaID,
            EstadoCodigo = cita.EstadoCita?.Codigo,
            EstadoNombre = cita.EstadoCita?.Nombre,

            FechaHoraInicio = cita.FechaHoraInicio,
            FechaHoraFin = cita.FechaHoraFin,
            Motivo = cita.Motivo,
            Observaciones = cita.Observaciones,
            FechaCreacion = cita.FechaCreacion
        };

        public static List<CitaDto> ToDtoList(this IEnumerable<Cita> citas) =>
            citas.Select(c => c.ToDto()).ToList();

        public static Cita ToEntity(this CrearCitaDto dto) => new Cita
        {
            PacienteID = dto.PacienteId,
            MedicoID = dto.MedicoId,
            EspecialidadID = dto.EspecialidadId,
            ConsultorioID = dto.ConsultorioId,
            EstadoCitaID = dto.EstadoCitaId,
            FechaHoraInicio = dto.FechaHoraInicio,
            FechaHoraFin = dto.FechaHoraFin,
            Motivo = dto.Motivo,
            Observaciones = dto.Observaciones,

            // La pone el servidor, nunca el cliente.
            FechaCreacion = DateTime.Now
        };

        /// <summary>
        /// Copia los campos editables del DTO sobre una cita que ya existe.
        /// No toca CitaID ni FechaCreacion: esos no se modifican nunca.
        /// </summary>
        public static void AplicarCambios(this ActualizarCitaDto dto, Cita cita)
        {
            cita.PacienteID = dto.PacienteId;
            cita.MedicoID = dto.MedicoId;
            cita.EspecialidadID = dto.EspecialidadId;
            cita.ConsultorioID = dto.ConsultorioId;
            cita.EstadoCitaID = dto.EstadoCitaId;
            cita.FechaHoraInicio = dto.FechaHoraInicio;
            cita.FechaHoraFin = dto.FechaHoraFin;
            cita.Motivo = dto.Motivo;
            cita.Observaciones = dto.Observaciones;
        }
    }
}
