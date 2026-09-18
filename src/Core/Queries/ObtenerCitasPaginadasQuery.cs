using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Domain.AgendaYAtencion;
using GenericPersistence.Repository;

namespace Core.Queries
{
    public class ObtenerCitasPaginadasQuery : IRequest<PagedResult<Cita>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ObtenerCitasPaginadasHandler : IRequestHandler<ObtenerCitasPaginadasQuery, PagedResult<Cita>>
    {
        private readonly IGenericRepository<Cita> _genericRepository;

        public ObtenerCitasPaginadasHandler(IGenericRepository<Cita> genericRepository)
        {
            _genericRepository = genericRepository;
        }

        public async Task<PagedResult<Cita>> Handle(ObtenerCitasPaginadasQuery request, CancellationToken cancellationToken)
        {
            return await _genericRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                cancellationToken: cancellationToken);
        }
    }
}