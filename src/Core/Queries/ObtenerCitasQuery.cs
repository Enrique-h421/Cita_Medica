using Core.Dtos;
using Core.Mappings;
using MediatR;

namespace Core.Queries
{
    /// <summary>Devuelve todas las citas, ya aplanadas a DTO.</summary>
    public class ObtenerCitasQuery : IRequest<List<CitaDto>>
    {
    }

    public class ObtenerCitasHandler : IRequestHandler<ObtenerCitasQuery, List<CitaDto>>
    {
        private readonly ICitaRepository _repository;

        public ObtenerCitasHandler(ICitaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CitaDto>> Handle(ObtenerCitasQuery request, CancellationToken cancellationToken)
        {
            var citas = await _repository.ObtenerTodasLasCitasAsync();
            return citas.ToDtoList();
        }
    }
}
