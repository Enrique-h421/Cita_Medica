using Domain.Models;

namespace Core.Services
{
    /// <summary>
    /// Lo que Agenda necesita del microservicio de Catálogos, dicho en términos de
    /// negocio. Core define el contrato; Infrastructure lo implementa con la
    /// librería REST. Core nunca sabe que esto viaja por HTTP.
    /// </summary>
    public interface ICatalogosService
    {
        // --- Consultas puntuales: se usan para validar antes de guardar. ---
        // Devuelven null cuando el registro no existe.
        // Lanzan DomainException cuando Catálogos no responde, porque en ese caso
        // no se puede afirmar que el dato sea inválido: simplemente no se sabe.

        Task<PacienteResumen?> ObtenerPacienteAsync(long pacienteId, CancellationToken cancellationToken = default);

        Task<MedicoResumen?> ObtenerMedicoAsync(int medicoId, CancellationToken cancellationToken = default);

        Task<EspecialidadResumen?> ObtenerEspecialidadAsync(int especialidadId, CancellationToken cancellationToken = default);

        // --- Listados completos: se usan para ponerle nombre a las citas. ---
        // Son tolerantes a propósito: si Catálogos está caído devuelven lista
        // vacía en lugar de fallar, para que la agenda siga mostrándose aunque
        // sea sin nombres. Un listado de citas no debe morir porque otro
        // microservicio esté apagado.

        Task<List<PacienteResumen>> ObtenerPacientesAsync(CancellationToken cancellationToken = default);

        Task<List<MedicoResumen>> ObtenerMedicosAsync(CancellationToken cancellationToken = default);

        Task<List<EspecialidadResumen>> ObtenerEspecialidadesAsync(CancellationToken cancellationToken = default);
    }
}
