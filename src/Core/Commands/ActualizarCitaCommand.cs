using Core.Dtos;
using Core.Exceptions;
using Core.Mappings;
using MediatR;

namespace Core.Commands
{
    /// <summary>Modifica una cita existente. El Id viene de la ruta, no del cuerpo.</summary>
    public class ActualizarCitaCommand : IRequest<bool>
    {
        public long CitaId { get; set; }
        public ActualizarCitaDto Cita { get; set; } = new ActualizarCitaDto();
    }

    public class ActualizarCitaHandler : IRequestHandler<ActualizarCitaCommand, bool>
    {
        private readonly ICitaRepository _repository;

        public ActualizarCitaHandler(ICitaRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(ActualizarCitaCommand request, CancellationToken cancellationToken)
        {
            var datos = request.Cita;

            if (datos.FechaHoraFin.HasValue && datos.FechaHoraFin <= datos.FechaHoraInicio)
            {
                throw new DomainException(
                    Errores.CITA_INVALIDA,
                    "La hora de fin debe ser posterior a la hora de inicio.");
            }

            // Se lee primero para no sobrescribir con nulos lo que el cliente no mandó.
            var cita = await _repository.ObtenerCitaPorIdAsync(request.CitaId);

            if (cita is null)
            {
                throw new DomainException(
                    Errores.CITA_NO_ENCONTRADA,
                    $"No existe una cita con el Id {request.CitaId}.");
            }

            datos.AplicarCambios(cita);
            await _repository.ActualizarCitaAsync(cita);

            return true;
        }
    }
}
