using Core.Dtos;
using Core.Exceptions;
using Core.Mappings;
using Domain.AgendaYAtencion;
using Domain.Organizacion;
using GenericPersistence.FiltroDinamico;
using GenericPersistence.Repository;
using MediatR;

namespace Core.Features.Consultorios
{
    // ======================= Queries =======================

    public class ObtenerConsultoriosQuery : IRequest<List<ConsultorioDto>> { }

    public class ObtenerConsultoriosHandler
        : IRequestHandler<ObtenerConsultoriosQuery, List<ConsultorioDto>>
    {
        private const int TopeListado = 1000;

        private readonly IGenericRepository<Consultorio> _repo;

        public ObtenerConsultoriosHandler(IGenericRepository<Consultorio> repo) => _repo = repo;

        public async Task<List<ConsultorioDto>> Handle(
            ObtenerConsultoriosQuery request, CancellationToken cancellationToken)
        {
            // Se usa la versión paginada porque es la que admite includes: sin
            // ellos la sucursal y el área vendrían en null.
            var pagina = await _repo.GetPagedAsync(
                1, TopeListado, null, null, "Codigo", true, false, cancellationToken,
                c => c.Sucursal!, c => c.Area!);

            return pagina.Data.ToDtoList();
        }
    }

    public class ObtenerConsultorioPorIdQuery : IRequest<ConsultorioDto>
    {
        public int ConsultorioId { get; set; }
    }

    public class ObtenerConsultorioPorIdHandler
        : IRequestHandler<ObtenerConsultorioPorIdQuery, ConsultorioDto>
    {
        private readonly IGenericRepository<Consultorio> _repo;
        private readonly IGenericRepository<Sucursal> _sucursales;
        private readonly IGenericRepository<Area> _areas;

        public ObtenerConsultorioPorIdHandler(
            IGenericRepository<Consultorio> repo,
            IGenericRepository<Sucursal> sucursales,
            IGenericRepository<Area> areas)
        {
            _repo = repo;
            _sucursales = sucursales;
            _areas = areas;
        }

        public async Task<ConsultorioDto> Handle(
            ObtenerConsultorioPorIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _repo.ObtenerPorIdAsync(request.ConsultorioId);

            if (item is null)
            {
                throw new DomainException(
                    Errores.CONSULTORIO_NO_ENCONTRADO,
                    $"No existe un consultorio con el Id {request.ConsultorioId}.");
            }

            var dto = item.ToDto();

            var sucursal = await _sucursales.ObtenerPorIdAsync(item.SucursalID);
            dto.SucursalNombre = sucursal?.Nombre;

            if (item.AreaID.HasValue)
            {
                var area = await _areas.ObtenerPorIdAsync(item.AreaID.Value);
                dto.AreaNombre = area?.Nombre;
            }

            return dto;
        }
    }

    public class ObtenerConsultoriosPaginadosQuery : IRequest<PagedResult<ConsultorioDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Filter { get; set; }
        public string? OrderBy { get; set; }
    }

    public class ObtenerConsultoriosPaginadosHandler
        : IRequestHandler<ObtenerConsultoriosPaginadosQuery, PagedResult<ConsultorioDto>>
    {
        private readonly IGenericRepository<Consultorio> _repo;

        public ObtenerConsultoriosPaginadosHandler(IGenericRepository<Consultorio> repo) => _repo = repo;

        public async Task<PagedResult<ConsultorioDto>> Handle(
            ObtenerConsultoriosPaginadosQuery request, CancellationToken cancellationToken)
        {
            var filtro = !string.IsNullOrWhiteSpace(request.Filter)
                ? Filter.FromStringExpression<Consultorio>(request.Filter)
                : null;

            var orden = string.IsNullOrWhiteSpace(request.OrderBy) ? "Codigo" : request.OrderBy;

            var pagina = await _repo.GetPagedAsync(
                request.PageNumber, request.PageSize, filtro, null, orden, true, false, cancellationToken,
                c => c.Sucursal!, c => c.Area!);

            return new PagedResult<ConsultorioDto>
            {
                Data = pagina.Data.ToDtoList(),
                TotalRecords = pagina.TotalRecords,
                PageSize = pagina.PageSize,
                CurrentPage = pagina.CurrentPage
            };
        }
    }

    // ======================= Commands =======================

    public class CrearConsultorioCommand : IRequest<ConsultorioDto>
    {
        public CrearConsultorioDto Consultorio { get; set; } = new();
    }

    public class CrearConsultorioHandler
        : IRequestHandler<CrearConsultorioCommand, ConsultorioDto>
    {
        private readonly IGenericRepository<Consultorio> _repo;
        private readonly IGenericRepository<Sucursal> _sucursales;
        private readonly IGenericRepository<Area> _areas;

        public CrearConsultorioHandler(
            IGenericRepository<Consultorio> repo,
            IGenericRepository<Sucursal> sucursales,
            IGenericRepository<Area> areas)
        {
            _repo = repo;
            _sucursales = sucursales;
            _areas = areas;
        }

        public async Task<ConsultorioDto> Handle(
            CrearConsultorioCommand request, CancellationToken cancellationToken)
        {
            var datos = request.Consultorio;

            // Se validan antes de guardar para que un FK roto no salga como 500.
            var sucursal = await _sucursales.ObtenerPorIdAsync(datos.SucursalId);

            if (sucursal is null)
            {
                throw new DomainException(
                    Errores.SUCURSAL_NO_ENCONTRADA,
                    $"No existe una sucursal con el Id {datos.SucursalId}.");
            }

            Area? area = null;

            if (datos.AreaId.HasValue)
            {
                area = await _areas.ObtenerPorIdAsync(datos.AreaId.Value);

                if (area is null)
                {
                    throw new DomainException(
                        Errores.AREA_NO_ENCONTRADA,
                        $"No existe un área con el Id {datos.AreaId.Value}.");
                }
            }

            var entidad = datos.ToEntity();
            await _repo.CrearAsync(entidad);

            var dto = entidad.ToDto();
            dto.SucursalNombre = sucursal.Nombre;
            dto.AreaNombre = area?.Nombre;

            return dto;
        }
    }

    public class ActualizarConsultorioCommand : IRequest<bool>
    {
        public int ConsultorioId { get; set; }
        public ActualizarConsultorioDto Consultorio { get; set; } = new();
    }

    public class ActualizarConsultorioHandler
        : IRequestHandler<ActualizarConsultorioCommand, bool>
    {
        private readonly IGenericRepository<Consultorio> _repo;
        private readonly IGenericRepository<Sucursal> _sucursales;
        private readonly IGenericRepository<Area> _areas;

        public ActualizarConsultorioHandler(
            IGenericRepository<Consultorio> repo,
            IGenericRepository<Sucursal> sucursales,
            IGenericRepository<Area> areas)
        {
            _repo = repo;
            _sucursales = sucursales;
            _areas = areas;
        }

        public async Task<bool> Handle(
            ActualizarConsultorioCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _repo.ObtenerPorIdAsync(request.ConsultorioId);

            if (entidad is null)
            {
                throw new DomainException(
                    Errores.CONSULTORIO_NO_ENCONTRADO,
                    $"No existe un consultorio con el Id {request.ConsultorioId}.");
            }

            var datos = request.Consultorio;

            if (await _sucursales.ObtenerPorIdAsync(datos.SucursalId) is null)
            {
                throw new DomainException(
                    Errores.SUCURSAL_NO_ENCONTRADA,
                    $"No existe una sucursal con el Id {datos.SucursalId}.");
            }

            if (datos.AreaId.HasValue && await _areas.ObtenerPorIdAsync(datos.AreaId.Value) is null)
            {
                throw new DomainException(
                    Errores.AREA_NO_ENCONTRADA,
                    $"No existe un área con el Id {datos.AreaId.Value}.");
            }

            datos.AplicarCambios(entidad);
            await _repo.ActualizarAsync(entidad);
            return true;
        }
    }

    public class EliminarConsultorioCommand : IRequest<bool>
    {
        public int ConsultorioId { get; set; }
    }

    public class EliminarConsultorioHandler
        : IRequestHandler<EliminarConsultorioCommand, bool>
    {
        private readonly IGenericRepository<Consultorio> _repo;

        public EliminarConsultorioHandler(IGenericRepository<Consultorio> repo) => _repo = repo;

        public async Task<bool> Handle(
            EliminarConsultorioCommand request, CancellationToken cancellationToken)
        {
            var entidad = await _repo.ObtenerPorIdAsync(request.ConsultorioId);

            if (entidad is null)
            {
                throw new DomainException(
                    Errores.CONSULTORIO_NO_ENCONTRADO,
                    $"No existe un consultorio con el Id {request.ConsultorioId}.");
            }

            await _repo.EliminarAsync(request.ConsultorioId);
            return true;
        }
    }
}
