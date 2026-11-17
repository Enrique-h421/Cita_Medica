using Core.Dtos;
using Domain.AgendaYAtencion;
using Domain.Expedientes;

namespace Core.Mappings
{
    /// <summary>
    /// Traduce entre las entidades de atención y expediente y los DTOs que viajan
    /// por la API. Igual que con las citas, se hace a mano: son pocos campos.
    /// </summary>
    public static class AtencionMapper
    {
        public static AtencionDto ToDto(this Atencion a) => new AtencionDto
        {
            AtencionId = a.AtencionID,
            PacienteId = a.PacienteID,
            MedicoId = a.MedicoID,
            CitaId = a.CitaID,
            ExpedienteId = a.ExpedienteID,
            NumeroExpediente = a.Expediente?.NumeroExpediente,
            FechaInicio = a.FechaInicio,
            FechaFin = a.FechaFin,
            DuracionMinutos = a.FechaFin.HasValue
                ? (int)(a.FechaFin.Value - a.FechaInicio).TotalMinutes
                : null,
            MotivoConsulta = a.MotivoConsulta,
            EnfermedadActual = a.EnfermedadActual,
            ExploracionFisica = a.ExploracionFisica,
            Observaciones = a.Observaciones,
            Estado = a.Estado
        };

        public static List<AtencionDto> ToDtoList(this IEnumerable<Atencion> items) =>
            items.Select(x => x.ToDto()).ToList();

        /// <summary>
        /// Arma la entidad a partir del DTO. El expediente y el estado no vienen
        /// del cliente: los pone el handler, porque son decisiones del sistema.
        /// </summary>
        public static Atencion ToEntity(this CrearAtencionDto dto) => new Atencion
        {
            PacienteID = dto.PacienteId,
            MedicoID = dto.MedicoId,
            CitaID = dto.CitaId,
            FechaInicio = dto.FechaInicio ?? DateTime.Now,
            MotivoConsulta = dto.MotivoConsulta,
            Observaciones = dto.Observaciones,
            Estado = EstadosAtencion.Abierta
        };

        /// <summary>
        /// Corrige lo documentado sin tocar fechas, estado ni expediente.
        /// Solo aplica lo que venga con valor: así se puede corregir un campo
        /// sin tener que reenviar los otros tres.
        /// </summary>
        public static void AplicarCambios(this ActualizarAtencionDto dto, Atencion a)
        {
            if (dto.MotivoConsulta is not null) a.MotivoConsulta = dto.MotivoConsulta;
            if (dto.EnfermedadActual is not null) a.EnfermedadActual = dto.EnfermedadActual;
            if (dto.ExploracionFisica is not null) a.ExploracionFisica = dto.ExploracionFisica;
            if (dto.Observaciones is not null) a.Observaciones = dto.Observaciones;
        }

        /// <summary>Escribe los hallazgos del cierre y deja la atención cerrada.</summary>
        public static void AplicarCierre(this CerrarAtencionDto dto, Atencion a, DateTime fechaFin)
        {
            if (dto.MotivoConsulta is not null) a.MotivoConsulta = dto.MotivoConsulta;
            if (dto.EnfermedadActual is not null) a.EnfermedadActual = dto.EnfermedadActual;
            if (dto.ExploracionFisica is not null) a.ExploracionFisica = dto.ExploracionFisica;
            if (dto.Observaciones is not null) a.Observaciones = dto.Observaciones;

            a.FechaFin = fechaFin;
            a.Estado = EstadosAtencion.Cerrada;
        }

        // ---------------- Expediente ----------------

        public static ExpedienteDto ToDto(this ExpedienteClinico e) => new ExpedienteDto
        {
            ExpedienteId = e.ExpedienteID,
            PacienteId = e.PacienteID,
            NumeroExpediente = e.NumeroExpediente,
            FechaApertura = e.FechaApertura,
            Observaciones = e.Observaciones,
            Activo = e.Activo,
            TotalAtenciones = e.Atenciones?.Count ?? 0
        };

        public static List<ExpedienteDto> ToDtoList(this IEnumerable<ExpedienteClinico> items) =>
            items.Select(x => x.ToDto()).ToList();
    }
}
