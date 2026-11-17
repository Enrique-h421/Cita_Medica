using Catalogos.Core.Dtos;
using Catalogos.Core.Exceptions;
using Catalogos.Core.Mappings;
using Catalogos.Domain.Entities;
using GenericPersistence.FiltroDinamico;
using GenericPersistence.Repository;
using MediatR;

namespace Catalogos.Core.Features.Pacientes
{
    // ======================= Queries =======================

    public class ObtenerPacientesQuery : IRequest<List<PacienteDto>> { }

    public class ObtenerPacientesHandler : IRequestHandler<ObtenerPacientesQuery, List<PacienteDto>>
    {
        private readonly IGenericRepository<Paciente> _repo;

        public ObtenerPacientesHandler(IGenericRepository<Paciente> repo) => _repo = repo;

        public async Task<List<PacienteDto>> Handle(
            ObtenerPacientesQuery request, CancellationToken cancellationToken)
        {
            var items = await _repo.ObtenerTodosAsync();
            return items.OrderBy(x => x.Apellidos).ThenBy(x => x.Nombres).ToDtoList();
        }
    }

    public class ObtenerPacientePorIdQuery : IRequest<PacienteDto>
    {
        public long PacienteId { get; set; }
    }

    public class ObtenerPacientePorIdHandler : IRequestHandler<ObtenerPacientePorIdQuery, PacienteDto>
    {
        private readonly IGenericRepository<Paciente> _repo;

        public ObtenerPacientePorIdHandler(IGenericRepository<Paciente> repo) => _repo = repo;

        public async Task<PacienteDto> Handle(
            ObtenerPacientePorIdQuery request, CancellationToken cancellationToken)
        {
            var paciente = await _repo.ObtenerPorIdAsync(request.PacienteId);

            if (paciente is null)
            {
                throw new DomainException(
                    Errores.PACIENTE_NO_ENCONTRADO,
                    $"No existe un paciente con el Id {request.PacienteId}.");
            }

            return paciente.ToDto();
        }
    }

    public class ObtenerPacientesPaginadosQuery : IRequest<PagedResult<PacienteDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Filter { get; set; }
        public string? OrderBy { get; set; }
    }

    public class ObtenerPacientesPaginadosHandler
        : IRequestHandler<ObtenerPacientesPaginadosQuery, PagedResult<PacienteDto>>
    {
        private readonly IGenericRepository<Paciente> _repo;

        public ObtenerPacientesPaginadosHandler(IGenericRepository<Paciente> repo) => _repo = repo;

        public async Task<PagedResult<PacienteDto>> Handle(
            ObtenerPacientesPaginadosQuery request, CancellationToken cancellationToken)
        {
            var filtro = !string.IsNullOrWhiteSpace(request.Filter)
                ? Filter.FromStringExpression<Paciente>(request.Filter)
                : null;

            var orden = string.IsNullOrWhiteSpace(request.OrderBy) ? "Apellidos" : request.OrderBy;

            var pagina = await _repo.GetPagedAsync(
                request.PageNumber, request.PageSize, filtro, null, orden, true, false, cancellationToken);

            return new PagedResult<PacienteDto>
            {
                Data = pagina.Data.ToDtoList(),
                TotalRecords = pagina.TotalRecords,
                PageSize = pagina.PageSize,
                CurrentPage = pagina.CurrentPage
            };
        }
    }

    // ======================= Commands =======================

    public class CrearPacienteCommand : IRequest<PacienteDto>
    {
        public CrearPacienteDto Paciente { get; set; } = new();
    }

    public class CrearPacienteHandler : IRequestHandler<CrearPacienteCommand, PacienteDto>
    {
        private readonly IGenericRepository<Paciente> _repo;

        public CrearPacienteHandler(IGenericRepository<Paciente> repo) => _repo = repo;

        public async Task<PacienteDto> Handle(
            CrearPacienteCommand request, CancellationToken cancellationToken)
        {
            var datos = request.Paciente;

            if (datos.FechaNacimiento.HasValue && datos.FechaNacimiento.Value.Date > DateTime.Today)
            {
                throw new DomainException(
                    Errores.DATOS_INVALIDOS,
                    "La fecha de nacimiento no puede estar en el futuro.");
            }

            var entidad = datos.ToEntity();
            await _repo.CrearAsync(entidad);

            return entidad.ToDto();
        }
    }

    public class ActualizarPacienteCommand : IRequest<bool>
    {
        public long PacienteId { get; set; }
        public ActualizarPacienteDto Paciente { get; set; } = new();
    }

    public class ActualizarPacienteHandler : IRequestHandler<ActualizarPacienteCommand, bool>
    {
        private readonly IGenericRepository<Paciente> _repo;

        public ActualizarPacienteHandler(IGenericRepository<Paciente> repo) => _repo = repo;

        public async Task<bool> Handle(
            ActualizarPacienteCommand request, CancellationToken cancellationToken)
        {
            if (request.Paciente.FechaNacimiento.HasValue
                && request.Paciente.FechaNacimiento.Value.Date > DateTime.Today)
            {
                throw new DomainException(
                    Errores.DATOS_INVALIDOS,
                    "La fecha de nacimiento no puede estar en el futuro.");
            }

            var entidad = await _repo.ObtenerPorIdAsync(request.PacienteId);

            if (entidad is null)
            {
                throw new DomainException(
                    Errores.PACIENTE_NO_ENCONTRADO,
                    $"No existe un paciente con el Id {request.PacienteId}.");
            }

            request.Paciente.AplicarCambios(entidad);
            await _repo.ActualizarAsync(entidad);
            return true;
        }
    }

    public class EliminarPacienteCommand : IRequest<bool>
    {
        public long PacienteId { get; set; }
    }

    public class EliminarPacienteHandler : IRequestHandler<EliminarPacienteCommand, bool>
    {
        private readonly IGenericRepository<Paciente> _repo;

        public EliminarPacienteHandler(IGenericRepository<Paciente> repo) => _repo = repo;

        public async Task<bool> Handle(
            EliminarPacienteCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _repo.ObtenerPorIdAsync(request.PacienteId);

            if (entidad is null)
            {
                throw new DomainException(
                    Errores.PACIENTE_NO_ENCONTRADO,
                    $"No existe un paciente con el Id {request.PacienteId}.");
            }

            await _repo.EliminarAsync(request.PacienteId);
            return true;
        }
    }
}
