using Core.Dtos;
using Core.Exceptions;
using Core.Mappings;
using MediatR;

namespace Core.Commands
{
    /// <summary>Crea una cita y devuelve cómo quedó guardada.</summary>
    public class CrearCitaCommand : IRequest<CitaDto>
    {
        public CrearCitaDto Cita { get; set; } = new CrearCitaDto();
    }

    public class CrearCitaHandler : IRequestHandler<CrearCitaCommand, CitaDto>
    {
        private readonly ICitaRepository _repository;

        public CrearCitaHandler(ICitaRepository repository)
        {
            _repository = repository;
        }

        public async Task<CitaDto> Handle(CrearCitaCommand request, CancellationToken cancellationToken)
        {
            var datos = request.Cita;

            if (datos.FechaHoraFin.HasValue && datos.FechaHoraFin <= datos.FechaHoraInicio)
            {
                throw new DomainException(
                    Errores.CITA_INVALIDA,
                    "La hora de fin debe ser posterior a la hora de inicio.");
            }

            var cita = datos.ToEntity();
            await _repository.CrearCitaAsync(cita);

            // Se vuelve a leer para traer el EstadoCita y devolver el nombre del estado.
            var guardada = await _repository.ObtenerCitaPorIdAsync(cita.CitaID);

            return guardada is not null ? guardada.ToDto() : cita.ToDto();
        }
    }
}
