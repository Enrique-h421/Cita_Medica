using Core.Dtos;
using Core.Exceptions;
using Core.Mappings;
using Core.Services;
using Domain.AgendaYAtencion;
using GenericPersistence.Repository;
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
        private readonly ICatalogosService _catalogos;
        private readonly IGenericRepository<Consultorio> _consultorios;

        public ActualizarCitaHandler(
            ICitaRepository repository,
            ICatalogosService catalogos,
            IGenericRepository<Consultorio> consultorios)
        {
            _repository = repository;
            _catalogos = catalogos;
            _consultorios = consultorios;
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

            // Misma validación contra Catálogos que al crear: reasignar una cita a
            // un médico que no existe sería tan inválido como crearla así.
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

            if (datos.ConsultorioId.HasValue)
            {
                var consultorio = await _consultorios.ObtenerPorIdAsync(datos.ConsultorioId.Value);

                if (consultorio is null)
                {
                    throw new DomainException(
                        Errores.CONSULTORIO_NO_ENCONTRADO,
                        $"No existe un consultorio con el Id {datos.ConsultorioId.Value}.");
                }
            }

            datos.AplicarCambios(cita);
            await _repository.ActualizarCitaAsync(cita);

            return true;
        }
    }
}
