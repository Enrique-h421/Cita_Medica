using Catalogos.Core.Dtos;
using Catalogos.Core.Exceptions;
using Catalogos.Core.Mappings;
using Catalogos.Domain.Entities;
using GenericPersistence.FiltroDinamico;
using GenericPersistence.Repository;
using MediatR;

namespace Catalogos.Core.Features.Medicos
{
    // ======================= Queries =======================

    public class ObtenerMedicosQuery : IRequest<List<MedicoDto>> { }

    public class ObtenerMedicosHandler : IRequestHandler<ObtenerMedicosQuery, List<MedicoDto>>
    {
        private const int TopeListado = 1000;

        private readonly IGenericRepository<Medico> _repo;

        public ObtenerMedicosHandler(IGenericRepository<Medico> repo) => _repo = repo;

        public async Task<List<MedicoDto>> Handle(
            ObtenerMedicosQuery request, CancellationToken cancellationToken)
        {
            // Se usa la versión paginada porque es la que admite include: sin el
            // include la especialidad vendría en null y el DTO saldría sin su nombre.
            var pagina = await _repo.GetPagedAsync(
                1, TopeListado, null, null, "Apellidos", true, false, cancellationToken,
                m => m.Especialidad!);

            return pagina.Data.ToDtoList();
        }
    }

    public class ObtenerMedicoPorIdQuery : IRequest<MedicoDto>
    {
        public int MedicoId { get; set; }
    }

    public class ObtenerMedicoPorIdHandler : IRequestHandler<ObtenerMedicoPorIdQuery, MedicoDto>
    {
        private readonly IGenericRepository<Medico> _repo;
        private readonly IGenericRepository<Especialidad> _especialidades;

        public ObtenerMedicoPorIdHandler(
            IGenericRepository<Medico> repo,
            IGenericRepository<Especialidad> especialidades)
        {
            _repo = repo;
            _especialidades = especialidades;
        }

        public async Task<MedicoDto> Handle(
            ObtenerMedicoPorIdQuery request, CancellationToken cancellationToken)
        {
            var medico = await _repo.ObtenerPorIdAsync(request.MedicoId);

            if (medico is null)
            {
                throw new DomainException(
                    Errores.MEDICO_NO_ENCONTRADO,
                    $"No existe un médico con el Id {request.MedicoId}.");
            }

            var dto = medico.ToDto();

            if (medico.EspecialidadID.HasValue)
            {
                var especialidad = await _especialidades.ObtenerPorIdAsync(medico.EspecialidadID.Value);
                dto.EspecialidadNombre = especialidad?.Nombre;
            }

            return dto;
        }
    }

    public class ObtenerMedicosPaginadosQuery : IRequest<PagedResult<MedicoDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Filter { get; set; }
        public string? OrderBy { get; set; }
    }

    public class ObtenerMedicosPaginadosHandler
        : IRequestHandler<ObtenerMedicosPaginadosQuery, PagedResult<MedicoDto>>
    {
        private readonly IGenericRepository<Medico> _repo;

        public ObtenerMedicosPaginadosHandler(IGenericRepository<Medico> repo) => _repo = repo;

        public async Task<PagedResult<MedicoDto>> Handle(
            ObtenerMedicosPaginadosQuery request, CancellationToken cancellationToken)
        {
            var filtro = !string.IsNullOrWhiteSpace(request.Filter)
                ? Filter.FromStringExpression<Medico>(request.Filter)
                : null;

            var orden = string.IsNullOrWhiteSpace(request.OrderBy) ? "Apellidos" : request.OrderBy;

            var pagina = await _repo.GetPagedAsync(
                request.PageNumber, request.PageSize, filtro, null, orden, true, false, cancellationToken,
                m => m.Especialidad!);

            return new PagedResult<MedicoDto>
            {
                Data = pagina.Data.ToDtoList(),
                TotalRecords = pagina.TotalRecords,
                PageSize = pagina.PageSize,
                CurrentPage = pagina.CurrentPage
            };
        }
    }

    // ======================= Commands =======================

    public class CrearMedicoCommand : IRequest<MedicoDto>
    {
        public CrearMedicoDto Medico { get; set; } = new();
    }

    public class CrearMedicoHandler : IRequestHandler<CrearMedicoCommand, MedicoDto>
    {
        private readonly IGenericRepository<Medico> _repo;
        private readonly IGenericRepository<Especialidad> _especialidades;

        public CrearMedicoHandler(
            IGenericRepository<Medico> repo,
            IGenericRepository<Especialidad> especialidades)
        {
            _repo = repo;
            _especialidades = especialidades;
        }

        public async Task<MedicoDto> Handle(
            CrearMedicoCommand request, CancellationToken cancellationToken)
        {
            var datos = request.Medico;

            // No se acepta una especialidad que no exista.
            if (datos.EspecialidadId.HasValue)
            {
                var existe = await _especialidades.ObtenerPorIdAsync(datos.EspecialidadId.Value);

                if (existe is null)
                {
                    throw new DomainException(
                        Errores.ESPECIALIDAD_NO_ENCONTRADA,
                        $"No existe una especialidad con el Id {datos.EspecialidadId.Value}.");
                }
            }

            var entidad = datos.ToEntity();
            await _repo.CrearAsync(entidad);

            var dto = entidad.ToDto();

            if (entidad.EspecialidadID.HasValue)
            {
                var especialidad = await _especialidades.ObtenerPorIdAsync(entidad.EspecialidadID.Value);
                dto.EspecialidadNombre = especialidad?.Nombre;
            }

            return dto;
        }
    }

    public class ActualizarMedicoCommand : IRequest<bool>
    {
        public int MedicoId { get; set; }
        public ActualizarMedicoDto Medico { get; set; } = new();
    }

    public class ActualizarMedicoHandler : IRequestHandler<ActualizarMedicoCommand, bool>
    {
        private readonly IGenericRepository<Medico> _repo;
        private readonly IGenericRepository<Especialidad> _especialidades;

        public ActualizarMedicoHandler(
            IGenericRepository<Medico> repo,
            IGenericRepository<Especialidad> especialidades)
        {
            _repo = repo;
            _especialidades = especialidades;
        }

        public async Task<bool> Handle(
            ActualizarMedicoCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _repo.ObtenerPorIdAsync(request.MedicoId);

            if (entidad is null)
            {
                throw new DomainException(
                    Errores.MEDICO_NO_ENCONTRADO,
                    $"No existe un médico con el Id {request.MedicoId}.");
            }

            if (request.Medico.EspecialidadId.HasValue)
            {
                var existe = await _especialidades.ObtenerPorIdAsync(request.Medico.EspecialidadId.Value);

                if (existe is null)
                {
                    throw new DomainException(
                        Errores.ESPECIALIDAD_NO_ENCONTRADA,
                        $"No existe una especialidad con el Id {request.Medico.EspecialidadId.Value}.");
                }
            }

            request.Medico.AplicarCambios(entidad);
            await _repo.ActualizarAsync(entidad);
            return true;
        }
    }

    public class EliminarMedicoCommand : IRequest<bool>
    {
        public int MedicoId { get; set; }
    }

    public class EliminarMedicoHandler : IRequestHandler<EliminarMedicoCommand, bool>
    {
        private readonly IGenericRepository<Medico> _repo;

        public EliminarMedicoHandler(IGenericRepository<Medico> repo) => _repo = repo;

        public async Task<bool> Handle(
            EliminarMedicoCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _repo.ObtenerPorIdAsync(request.MedicoId);

            if (entidad is null)
            {
                throw new DomainException(
                    Errores.MEDICO_NO_ENCONTRADO,
                    $"No existe un médico con el Id {request.MedicoId}.");
            }

            await _repo.EliminarAsync(request.MedicoId);
            return true;
        }
    }
}
