using Catalogos.Core.Dtos;
using Catalogos.Core.Exceptions;
using Catalogos.Core.Mappings;
using Catalogos.Core.Repositories;
using Catalogos.Domain.Entities;
using GenericPersistence.FiltroDinamico;
using GenericPersistence.Repository;
using MediatR;

namespace Catalogos.Core.Features.Medicos
{
    /// <summary>
    /// Resuelve las especialidades de los médicos desde la tabla puente.
    /// Se hace en memoria, con una consulta a cada catálogo, porque el
    /// repositorio genérico no soporta includes anidados. Para el tamaño de
    /// estos catálogos es irrelevante y evita tocar la librería.
    /// </summary>
    internal static class EspecialidadesResolver
    {
        public static async Task EnriquecerAsync(
            List<MedicoDto> medicos,
            IMedicoEspecialidadRepository relaciones,
            IGenericRepository<Especialidad> especialidades,
            CancellationToken cancellationToken = default)
        {
            if (medicos.Count == 0) return;

            var todasRelaciones = await relaciones.ObtenerTodasAsync(cancellationToken);

            var todasEspecialidades = (await especialidades.ObtenerTodosAsync())
                .GroupBy(e => e.EspecialidadID)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var medico in medicos)
            {
                var suyas = todasRelaciones.Where(r => r.MedicoID == medico.MedicoId).ToList();

                medico.Especialidades = suyas
                    .Where(r => todasEspecialidades.ContainsKey(r.EspecialidadID))
                    .Select(r => todasEspecialidades[r.EspecialidadID].Nombre)
                    .OrderBy(n => n)
                    .ToList();

                // La principal, o la primera que tenga si ninguna está marcada.
                var principal = suyas.FirstOrDefault(r => r.Principal) ?? suyas.FirstOrDefault();

                if (principal is not null
                    && todasEspecialidades.TryGetValue(principal.EspecialidadID, out var esp))
                {
                    medico.EspecialidadPrincipalId = esp.EspecialidadID;
                    medico.EspecialidadPrincipalNombre = esp.Nombre;
                }
            }
        }
    }

    // ======================= Queries =======================

    public class ObtenerMedicosQuery : IRequest<List<MedicoDto>> { }

    public class ObtenerMedicosHandler : IRequestHandler<ObtenerMedicosQuery, List<MedicoDto>>
    {
        private readonly IGenericRepository<Medico> _repo;
        private readonly IMedicoEspecialidadRepository _relaciones;
        private readonly IGenericRepository<Especialidad> _especialidades;

        public ObtenerMedicosHandler(
            IGenericRepository<Medico> repo,
            IMedicoEspecialidadRepository relaciones,
            IGenericRepository<Especialidad> especialidades)
        {
            _repo = repo;
            _relaciones = relaciones;
            _especialidades = especialidades;
        }

        public async Task<List<MedicoDto>> Handle(
            ObtenerMedicosQuery request, CancellationToken cancellationToken)
        {
            var items = await _repo.ObtenerTodosAsync();
            var dtos = items.OrderBy(x => x.Apellidos).ThenBy(x => x.Nombres).ToDtoList();

            await EspecialidadesResolver.EnriquecerAsync(dtos, _relaciones, _especialidades, cancellationToken);

            return dtos;
        }
    }

    public class ObtenerMedicoPorIdQuery : IRequest<MedicoDto>
    {
        public int MedicoId { get; set; }
    }

    public class ObtenerMedicoPorIdHandler : IRequestHandler<ObtenerMedicoPorIdQuery, MedicoDto>
    {
        private readonly IGenericRepository<Medico> _repo;
        private readonly IMedicoEspecialidadRepository _relaciones;
        private readonly IGenericRepository<Especialidad> _especialidades;

        public ObtenerMedicoPorIdHandler(
            IGenericRepository<Medico> repo,
            IMedicoEspecialidadRepository relaciones,
            IGenericRepository<Especialidad> especialidades)
        {
            _repo = repo;
            _relaciones = relaciones;
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

            var dtos = new List<MedicoDto> { medico.ToDto() };
            await EspecialidadesResolver.EnriquecerAsync(dtos, _relaciones, _especialidades, cancellationToken);

            return dtos[0];
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
        private readonly IMedicoEspecialidadRepository _relaciones;
        private readonly IGenericRepository<Especialidad> _especialidades;

        public ObtenerMedicosPaginadosHandler(
            IGenericRepository<Medico> repo,
            IMedicoEspecialidadRepository relaciones,
            IGenericRepository<Especialidad> especialidades)
        {
            _repo = repo;
            _relaciones = relaciones;
            _especialidades = especialidades;
        }

        public async Task<PagedResult<MedicoDto>> Handle(
            ObtenerMedicosPaginadosQuery request, CancellationToken cancellationToken)
        {
            var filtro = !string.IsNullOrWhiteSpace(request.Filter)
                ? Filter.FromStringExpression<Medico>(request.Filter)
                : null;

            var orden = string.IsNullOrWhiteSpace(request.OrderBy) ? "Apellidos" : request.OrderBy;

            var pagina = await _repo.GetPagedAsync(
                request.PageNumber, request.PageSize, filtro, null, orden, true, false, cancellationToken);

            var dtos = pagina.Data.ToDtoList();
            await EspecialidadesResolver.EnriquecerAsync(dtos, _relaciones, _especialidades, cancellationToken);

            return new PagedResult<MedicoDto>
            {
                Data = dtos,
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
        private readonly IMedicoEspecialidadRepository _relaciones;
        private readonly IGenericRepository<Especialidad> _especialidades;

        public CrearMedicoHandler(
            IGenericRepository<Medico> repo,
            IMedicoEspecialidadRepository relaciones,
            IGenericRepository<Especialidad> especialidades)
        {
            _repo = repo;
            _relaciones = relaciones;
            _especialidades = especialidades;
        }

        public async Task<MedicoDto> Handle(
            CrearMedicoCommand request, CancellationToken cancellationToken)
        {
            var datos = request.Medico;

            await ValidarEspecialidadesAsync(datos.EspecialidadesIds, _especialidades);

            var entidad = datos.ToEntity();
            await _repo.CrearAsync(entidad);

            if (datos.EspecialidadesIds.Count > 0)
            {
                await _relaciones.AsignarAsync(
                    entidad.MedicoID, datos.EspecialidadesIds, datos.EspecialidadPrincipalId, cancellationToken);
            }

            var dtos = new List<MedicoDto> { entidad.ToDto() };
            await EspecialidadesResolver.EnriquecerAsync(dtos, _relaciones, _especialidades, cancellationToken);

            return dtos[0];
        }

        internal static async Task ValidarEspecialidadesAsync(
            IEnumerable<int> ids, IGenericRepository<Especialidad> especialidades)
        {
            foreach (var id in ids.Distinct())
            {
                if (await especialidades.ObtenerPorIdAsync(id) is null)
                {
                    throw new DomainException(
                        Errores.ESPECIALIDAD_NO_ENCONTRADA,
                        $"No existe una especialidad con el Id {id}.");
                }
            }
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
        private readonly IMedicoEspecialidadRepository _relaciones;
        private readonly IGenericRepository<Especialidad> _especialidades;

        public ActualizarMedicoHandler(
            IGenericRepository<Medico> repo,
            IMedicoEspecialidadRepository relaciones,
            IGenericRepository<Especialidad> especialidades)
        {
            _repo = repo;
            _relaciones = relaciones;
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

            var datos = request.Medico;

            await CrearMedicoHandler.ValidarEspecialidadesAsync(datos.EspecialidadesIds, _especialidades);

            datos.AplicarCambios(entidad);
            await _repo.ActualizarAsync(entidad);

            await _relaciones.AsignarAsync(
                request.MedicoId, datos.EspecialidadesIds, datos.EspecialidadPrincipalId, cancellationToken);

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
        private readonly IMedicoEspecialidadRepository _relaciones;

        public EliminarMedicoHandler(
            IGenericRepository<Medico> repo,
            IMedicoEspecialidadRepository relaciones)
        {
            _repo = repo;
            _relaciones = relaciones;
        }

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

            // Primero las filas de la tabla puente, si no la FK lo impide.
            await _relaciones.EliminarPorMedicoAsync(request.MedicoId, cancellationToken);

            await _repo.EliminarAsync(request.MedicoId);
            return true;
        }
    }
}
