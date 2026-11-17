using Core.Dtos;
using Core.Services;

namespace Core.Mappings
{
    /// <summary>
    /// Le pone nombre a las atenciones y a los expedientes consultando el
    /// microservicio de Catálogos. Mismo criterio que con las citas: dos
    /// llamadas en total, no dos por registro, y si Catálogos está caído los
    /// nombres quedan en null en lugar de tumbar la respuesta.
    /// </summary>
    public static class AtencionesEnricher
    {
        public static async Task EnriquecerAsync(
            this List<AtencionDto> atenciones,
            ICatalogosService catalogos,
            CancellationToken cancellationToken = default)
        {
            if (atenciones.Count == 0) return;

            var pacientes = await catalogos.ObtenerPacientesAsync(cancellationToken);
            var medicos = await catalogos.ObtenerMedicosAsync(cancellationToken);

            var porPaciente = pacientes
                .GroupBy(p => p.PacienteId)
                .ToDictionary(g => g.Key, g => g.First());

            var porMedico = medicos
                .GroupBy(m => m.MedicoId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var atencion in atenciones)
            {
                if (porPaciente.TryGetValue(atencion.PacienteId, out var paciente))
                {
                    atencion.PacienteNombre = paciente.NombreCompleto;
                }

                if (porMedico.TryGetValue(atencion.MedicoId, out var medico))
                {
                    atencion.MedicoNombre = medico.NombreCompleto;
                }
            }
        }

        public static async Task EnriquecerAsync(
            this List<ExpedienteDto> expedientes,
            ICatalogosService catalogos,
            CancellationToken cancellationToken = default)
        {
            if (expedientes.Count == 0) return;

            var pacientes = await catalogos.ObtenerPacientesAsync(cancellationToken);

            var porPaciente = pacientes
                .GroupBy(p => p.PacienteId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var expediente in expedientes)
            {
                if (porPaciente.TryGetValue(expediente.PacienteId, out var paciente))
                {
                    expediente.PacienteNombre = paciente.NombreCompleto;
                }
            }
        }
    }
}
