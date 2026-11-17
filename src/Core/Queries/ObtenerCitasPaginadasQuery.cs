using Core.Dtos;
using Core.Mappings;
using Core.Services;
using Domain.AgendaYAtencion;
using GenericPersistence.FiltroDinamico;
using GenericPersistence.Repository;
using MediatR;

namespace Core.Queries
{
    /// <summary>
    /// Página de citas, con filtro dinámico opcional y orden configurable.
    /// Usa el repositorio genérico, no el específico.
    /// </summary>
    public class ObtenerCitasPaginadasQuery : IRequest<PagedResult<CitaDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        /// <summary>Expresión dinámica, por ejemplo: EstadoCitaID == 1</summary>
        public string? Filter { get; set; }

        /// <summary>Campo y sentido, por ejemplo: FechaHoraInicio descending</summary>
        public string? OrderBy { get; set; }
    }

    public class ObtenerCitasPaginadasHandler
        : IRequestHandler<ObtenerCitasPaginadasQuery, PagedResult<CitaDto>>
    {
        private const string OrdenPorDefecto = "FechaHoraInicio descending";

        private readonly IGenericRepository<Cita> _genericRepository;
        private readonly ICatalogosService _catalogos;

        public ObtenerCitasPaginadasHandler(
            IGenericRepository<Cita> genericRepository,
            ICatalogosService catalogos)
        {
            _genericRepository = genericRepository;
            _catalogos = catalogos;
        }

        public async Task<PagedResult<CitaDto>> Handle(
            ObtenerCitasPaginadasQuery request,
            CancellationToken cancellationToken)
        {
            var filtro = !string.IsNullOrWhiteSpace(request.Filter)
                ? Filter.FromStringExpression<Cita>(request.Filter)
                : null;

            var orden = string.IsNullOrWhiteSpace(request.OrderBy)
                ? OrdenPorDefecto
                : request.OrderBy;

            // El último argumento es el include: sin él EstadoCita viene en null
            // y el DTO saldría sin el nombre del estado.
            var pagina = await _genericRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                filtro,
                null,
                orden,
                true,
                false,
                cancellationToken,
                c => c.EstadoCita!);

            var dtos = pagina.Data.ToDtoList();

            await dtos.EnriquecerAsync(_catalogos, cancellationToken);

            return new PagedResult<CitaDto>
            {
                Data = dtos,
                TotalRecords = pagina.TotalRecords,
                PageSize = pagina.PageSize,
                CurrentPage = pagina.CurrentPage
            };
        }
    }
}
