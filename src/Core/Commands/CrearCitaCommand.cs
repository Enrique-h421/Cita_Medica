using Core.Dtos;
using Core.Exceptions;
using Core.Mappings;
using Core.Services;
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
        private readonly ICatalogosService _catalogos;

        public CrearCitaHandler(ICitaRepository repository, ICatalogosService catalogos)
        {
            _repository = repository;
            _catalogos = catalogos;
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

            // El paciente y el médico viven en otro microservicio. Agenda no tiene
            // llave foránea contra ellos, así que la integridad se valida preguntando.
            var paciente = await _catalogos.ObtenerPacienteAsync(datos.PacienteId, cancellationToken);

            if (paciente is null)
            {
                throw new DomainException(
                    Errores.PACIENTE_NO_ENCONTRADO,
                    $"No existe un paciente con el Id {datos.PacienteId} en el catálogo.");
            }

            var medico = await _catalogos.ObtenerMedicoAsync(datos.MedicoId, cancellationToken);

            if (medico is null)
            {
                throw new DomainException(
                    Errores.MEDICO_NO_ENCONTRADO,
                    $"No existe un médico con el Id {datos.MedicoId} en el catálogo.");
            }

            if (datos.EspecialidadId.HasValue)
            {
                var especialidad = await _catalogos.ObtenerEspecialidadAsync(
                    datos.EspecialidadId.Value, cancellationToken);

                if (especialidad is null)
                {
                    throw new DomainException(
                        Errores.ESPECIALIDAD_NO_ENCONTRADA,
                        $"No existe una especialidad con el Id {datos.EspecialidadId.Value} en el catálogo.");
                }
            }

            var cita = datos.ToEntity();
            await _repository.CrearCitaAsync(cita);

            // Se vuelve a leer para traer el EstadoCita y devolver el nombre del estado.
            var guardada = await _repository.ObtenerCitaPorIdAsync(cita.CitaID);
            var dto = guardada is not null ? guardada.ToDto() : cita.ToDto();

            // Los nombres ya se consultaron arriba durante la validación: se reusan
            // en lugar de volver a llamar a Catálogos.
            dto.PacienteNombre = paciente.NombreCompleto;
            dto.MedicoNombre = medico.NombreCompleto;
            dto.EspecialidadNombre ??= medico.EspecialidadNombre;

            return dto;
        }
    }
}
