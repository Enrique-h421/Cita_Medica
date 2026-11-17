using Core.Exceptions;
using MediatR;

namespace Core.Commands
{
    /// <summary>Elimina una cita. Si no existe avisa en vez de fallar en silencio.</summary>
    public class EliminarCitaCommand : IRequest<bool>
    {
        public long CitaId { get; set; }
    }

    public class EliminarCitaHandler : IRequestHandler<EliminarCitaCommand, bool>
    {
        private readonly ICitaRepository _repository;

        public EliminarCitaHandler(ICitaRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(EliminarCitaCommand request, CancellationToken cancellationToken)
        {
            var cita = await _repository.ObtenerCitaPorIdAsync(request.CitaId);

            if (cita is null)
            {
                throw new DomainException(
                    Errores.CITA_NO_ENCONTRADA,
                    $"No existe una cita con el Id {request.CitaId}.");
            }

            await _repository.EliminarCitaAsync(request.CitaId);

            return true;
        }
    }
}
