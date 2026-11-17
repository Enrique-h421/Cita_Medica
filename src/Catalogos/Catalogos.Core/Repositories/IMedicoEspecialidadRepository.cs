using Catalogos.Domain.Entities;

namespace Catalogos.Core.Repositories
{
    /// <summary>
    /// La tabla puente Medico_Especialidad tiene llave compuesta, y el
    /// repositorio genérico trabaja con llaves de una sola columna. En lugar de
    /// forzar la librería, este contrato específico encapsula el manejo de la
    /// relación completa: asignar el conjunto de especialidades de un médico.
    /// </summary>
    public interface IMedicoEspecialidadRepository
    {
        Task<List<MedicoEspecialidad>> ObtenerTodasAsync(CancellationToken cancellationToken = default);

        Task<List<MedicoEspecialidad>> ObtenerPorMedicoAsync(int medicoId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deja al médico exactamente con las especialidades indicadas: agrega
        /// las que falten, quita las que sobren y marca la principal.
        /// </summary>
        Task AsignarAsync(
            int medicoId,
            IEnumerable<int> especialidadIds,
            int? especialidadPrincipalId,
            CancellationToken cancellationToken = default);

        Task EliminarPorMedicoAsync(int medicoId, CancellationToken cancellationToken = default);
    }
}
