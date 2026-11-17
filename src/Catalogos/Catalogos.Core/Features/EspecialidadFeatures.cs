using Catalogos.Core.Dtos;
using Catalogos.Core.Exceptions;
using Catalogos.Core.Mappings;
using Catalogos.Domain.Entities;
using GenericPersistence.FiltroDinamico;
using GenericPersistence.Repository;
using MediatR;

namespace Catalogos.Core.Features.Especialidades
{
    // ======================= Queries =======================

    public class ObtenerEspecialidadesQuery : IRequest<List<EspecialidadDto>> { }

    public class ObtenerEspecialidadesHandler
        : IRequestHandler<ObtenerEspecialidadesQuery, List<EspecialidadDto>>
    {
        private readonly IGenericRepository<Especialidad> _repo;

        public ObtenerEspecialidadesHandler(IGenericRepository<Especialidad> repo) => _repo = repo;

        public async Task<List<EspecialidadDto>> Handle(
            ObtenerEspecialidadesQuery request, CancellationToken cancellationToken)
        {
            var items = await _repo.ObtenerTodosAsync();
            return items.OrderBy(x => x.Nombre).ToDtoList();
        }
    }

    public class ObtenerEspecialidadPorIdQuery : IRequest<EspecialidadDto>
    {
        public int EspecialidadId { get; set; }
    }

    public class ObtenerEspecialidadPorIdHandler
        : IRequestHandler<ObtenerEspecialidadPorIdQuery, EspecialidadDto>
    {
        private readonly IGenericRepository<Especialidad> _repo;

        public ObtenerEspecialidadPorIdHandler(IGenericRepository<Especialidad> repo) => _repo = repo;

        public async Task<EspecialidadDto> Handle(
            ObtenerEspecialidadPorIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _repo.ObtenerPorIdAsync(request.EspecialidadId);

            if (item is null)
            {
                throw new DomainException(
                    Errores.ESPECIALIDAD_NO_ENCONTRADA,
                    $"No existe una especialidad con el Id {request.EspecialidadId}.");
            }

            return item.ToDto();
        }
    }

    public class ObtenerEspecialidadesPaginadasQuery : IRequest<PagedResult<EspecialidadDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Filter { get; set; }
        public string? OrderBy { get; set; }
    }

    public class ObtenerEspecialidadesPaginadasHandler
        : IRequestHandler<ObtenerEspecialidadesPaginadasQuery, PagedResult<EspecialidadDto>>
    {
        private readonly IGenericRepository<Especialidad> _repo;

        public ObtenerEspecialidadesPaginadasHandler(IGenericRepository<Especialidad> repo) => _repo = repo;

        public async Task<PagedResult<EspecialidadDto>> Handle(
            ObtenerEspecialidadesPaginadasQuery request, CancellationToken cancellationToken)
        {
            var filtro = !string.IsNullOrWhiteSpace(request.Filter)
                ? Filter.FromStringExpression<Especialidad>(request.Filter)
                : null;

            var orden = string.IsNullOrWhiteSpace(request.OrderBy) ? "Nombre" : request.OrderBy;

            var pagina = await _repo.GetPagedAsync(
                request.PageNumber, request.PageSize, filtro, null, orden, true, false, cancellationToken);

            return new PagedResult<EspecialidadDto>
            {
                Data = pagina.Data.ToDtoList(),
                TotalRecords = pagina.TotalRecords,
                PageSize = pagina.PageSize,
                CurrentPage = pagina.CurrentPage
            };
        }
    }

    // ======================= Commands =======================

    public class CrearEspecialidadCommand : IRequest<EspecialidadDto>
    {
        public CrearEspecialidadDto Especialidad { get; set; } = new();
    }

    public class CrearEspecialidadHandler
        : IRequestHandler<CrearEspecialidadCommand, EspecialidadDto>
    {
        private readonly IGenericRepository<Especialidad> _repo;

        public CrearEspecialidadHandler(IGenericRepository<Especialidad> repo) => _repo = repo;

        public async Task<EspecialidadDto> Handle(
            CrearEspecialidadCommand request, CancellationToken cancellationToken)
        {
            var entidad = request.Especialidad.ToEntity();
            await _repo.CrearAsync(entidad);
            return entidad.ToDto();
        }
    }

    public class ActualizarEspecialidadCommand : IRequest<bool>
    {
        public int EspecialidadId { get; set; }
        public ActualizarEspecialidadDto Especialidad { get; set; } = new();
    }

    public class ActualizarEspecialidadHandler
        : IRequestHandler<ActualizarEspecialidadCommand, bool>
    {
        private readonly IGenericRepository<Especialidad> _repo;

        public ActualizarEspecialidadHandler(IGenericRepository<Especialidad> repo) => _repo = repo;

        public async Task<bool> Handle(
            ActualizarEspecialidadCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _repo.ObtenerPorIdAsync(request.EspecialidadId);

            if (entidad is null)
            {
                throw new DomainException(
                    Errores.ESPECIALIDAD_NO_ENCONTRADA,
                    $"No existe una especialidad con el Id {request.EspecialidadId}.");
            }

            request.Especialidad.AplicarCambios(entidad);
            await _repo.ActualizarAsync(entidad);
            return true;
        }
    }

    public class EliminarEspecialidadCommand : IRequest<bool>
    {
        public int EspecialidadId { get; set; }
    }

    public class EliminarEspecialidadHandler
        : IRequestHandler<EliminarEspecialidadCommand, bool>
    {
        private readonly IGenericRepository<Especialidad> _repo;

        public EliminarEspecialidadHandler(IGenericRepository<Especialidad> repo) => _repo = repo;

        public async Task<bool> Handle(
            EliminarEspecialidadCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _repo.ObtenerPorIdAsync(request.EspecialidadId);

            if (entidad is null)
            {
                throw new DomainException(
                    Errores.ESPECIALIDAD_NO_ENCONTRADA,
                    $"No existe una especialidad con el Id {request.EspecialidadId}.");
            }

            await _repo.EliminarAsync(request.EspecialidadId);
            return true;
        }
    }
}
