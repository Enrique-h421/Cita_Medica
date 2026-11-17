using Core.Dtos;
using Core.Services;

namespace Core.Mappings
{
    /// <summary>
    /// Le pone nombre a las citas consultando el microservicio de Catálogos.
    /// </summary>
    public static class CitasEnricher
    {
        /// <summary>
        /// Hace tres llamadas en total, no tres por cita: se traen los catálogos
        /// completos una sola vez y el cruce se hace en memoria. Con 50 citas en
        /// pantalla la diferencia es entre 3 llamadas HTTP y 150.
        ///
        /// Si Catálogos no responde, los nombres quedan en null y las citas se
        /// devuelven igual. Degradar es mejor que fallar: el usuario prefiere ver
        /// su agenda con identificadores a no ver nada.
        /// </summary>
        public static async Task EnriquecerAsync(
            this List<CitaDto> citas,
            ICatalogosService catalogos,
            CancellationToken cancellationToken = default)
        {
            if (citas.Count == 0) return;

            var pacientes = await catalogos.ObtenerPacientesAsync(cancellationToken);
            var medicos = await catalogos.ObtenerMedicosAsync(cancellationToken);
            var especialidades = await catalogos.ObtenerEspecialidadesAsync(cancellationToken);

            var porPaciente = pacientes
                .GroupBy(p => p.PacienteId)
                .ToDictionary(g => g.Key, g => g.First());

            var porMedico = medicos
                .GroupBy(m => m.MedicoId)
                .ToDictionary(g => g.Key, g => g.First());

            var porEspecialidad = especialidades
                .GroupBy(e => e.EspecialidadId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var cita in citas)
            {
                if (porPaciente.TryGetValue(cita.PacienteId, out var paciente))
                {
                    cita.PacienteNombre = paciente.NombreCompleto;
                }

                if (porMedico.TryGetValue(cita.MedicoId, out var medico))
                {
                    cita.MedicoNombre = medico.NombreCompleto;
                }

                if (cita.EspecialidadId.HasValue
                    && porEspecialidad.TryGetValue(cita.EspecialidadId.Value, out var especialidad))
                {
                    cita.EspecialidadNombre = especialidad.Nombre;
                }
                else if (!cita.EspecialidadId.HasValue && medico is not null)
                {
                    // La cita no trae especialidad propia: se muestra la del médico
                    // que la atiende, que es la información útil en pantalla.
                    cita.EspecialidadNombre = medico.EspecialidadNombre;
                }
            }
        }
    }
}
