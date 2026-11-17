using Core.Dtos;
using Core.Mappings;
using Core.Services;
using MediatR;

namespace Core.Queries
{
    /// <summary>Devuelve todas las citas, aplanadas a DTO y con los nombres
    /// resueltos contra el microservicio de Catálogos.</summary>
    public class ObtenerCitasQuery : IRequest<List<CitaDto>>
    {
    }

    public class ObtenerCitasHandler : IRequestHandler<ObtenerCitasQuery, List<CitaDto>>
    {
        private readonly ICitaRepository _repository;
        private readonly ICatalogosService _catalogos;

        public ObtenerCitasHandler(ICitaRepository repository, ICatalogosService catalogos)
        {
            _repository = repository;
            _catalogos = catalogos;
        }

        public async Task<List<CitaDto>> Handle(ObtenerCitasQuery request, CancellationToken cancellationToken)
        {
            var citas = await _repository.ObtenerTodasLasCitasAsync();
            var dtos = citas.ToDtoList();

            await dtos.EnriquecerAsync(_catalogos, cancellationToken);

            return dtos;
        }
    }
}
