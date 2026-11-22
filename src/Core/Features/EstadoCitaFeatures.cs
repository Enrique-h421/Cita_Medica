using Core.Dtos;
using Domain.AgendaYAtencion;
using GenericPersistence.Repository;
using MediatR;

namespace Core.Features.EstadosCita
{
    /// <summary>
    /// El catálogo de estados por los que pasa una cita.
    ///
    /// Es solo lectura: los estados los define la clínica una vez y se siembran
    /// por SQL. Si alguna vez hiciera falta agregarlos desde la aplicación,
    /// sería una pantalla de administración aparte, no algo que toque quien
    /// agenda una cita.
    /// </summary>
    public class ObtenerEstadosCitaQuery : IRequest<List<EstadoCitaDto>> { }

    public class ObtenerEstadosCitaHandler
        : IRequestHandler<ObtenerEstadosCitaQuery, List<EstadoCitaDto>>
    {
        private readonly IGenericRepository<EstadoCita> _repo;

        public ObtenerEstadosCitaHandler(IGenericRepository<EstadoCita> repo) => _repo = repo;

        public async Task<List<EstadoCitaDto>> Handle(
            ObtenerEstadosCitaQuery request, CancellationToken cancellationToken)
        {
            var estados = await _repo.ObtenerTodosAsync();

            return estados
                .OrderBy(e => e.EstadoCitaID)
                .Select(e => new EstadoCitaDto
                {
                    EstadoCitaId = e.EstadoCitaID,
                    Codigo = e.Codigo,
                    Nombre = e.Nombre,
                })
                .ToList();
        }
    }
}
