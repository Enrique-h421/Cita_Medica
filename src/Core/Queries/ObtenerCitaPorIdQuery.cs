using Core.Dtos;
using Core.Exceptions;
using Core.Mappings;
using MediatR;

namespace Core.Queries
{
    /// <summary>Devuelve una cita por su Id. Si no existe lanza DomainException,
    /// que el middleware de la API convierte en un 404 con el sobre HttpResponse.</summary>
    public class ObtenerCitaPorIdQuery : IRequest<CitaDto>
    {
        public long CitaId { get; set; }
    }

    public class ObtenerCitaPorIdHandler : IRequestHandler<ObtenerCitaPorIdQuery, CitaDto>
    {
        private readonly ICitaRepository _repository;

        public ObtenerCitaPorIdHandler(ICitaRepository repository)
        {
            _repository = repository;
        }

        public async Task<CitaDto> Handle(ObtenerCitaPorIdQuery request, CancellationToken cancellationToken)
        {
            var cita = await _repository.ObtenerCitaPorIdAsync(request.CitaId);

            if (cita is null)
            {
                throw new DomainException(
                    Errores.CITA_NO_ENCONTRADA,
                    $"No existe una cita con el Id {request.CitaId}.");
            }

            return cita.ToDto();
        }
    }
}
