using Core.Dtos;
using Core.Exceptions;
using Core.Mappings;
using Core.Services;
using Domain.Expedientes;
using GenericPersistence.FiltroDinamico;
using GenericPersistence.Repository;
using MediatR;

namespace Core.Features.Expedientes
{
    /// <summary>
    /// Consultas del expediente clínico. Solo lectura: el expediente se abre
    /// solo en la primera atención del paciente y no se borra nunca.
    /// </summary>
    public class ObtenerExpedientesQuery : IRequest<List<ExpedienteDto>> { }

    public class ObtenerExpedientesHandler
        : IRequestHandler<ObtenerExpedientesQuery, List<ExpedienteDto>>
    {
        private const int TopeListado = 1000;

        private readonly IGenericRepository<ExpedienteClinico> _repo;
        private readonly ICatalogosService _catalogos;

        public ObtenerExpedientesHandler(
            IGenericRepository<ExpedienteClinico> repo, ICatalogosService catalogos)
        {
            _repo = repo;
            _catalogos = catalogos;
        }

        public async Task<List<ExpedienteDto>> Handle(
            ObtenerExpedientesQuery request, CancellationToken cancellationToken)
        {
            // splitQuery en true: al traer las atenciones de cada expediente, una
            // sola consulta con JOIN repetiría la fila del expediente por cada
            // atención. Dos consultas separadas salen más baratas.
            var pagina = await _repo.GetPagedAsync(
                1, TopeListado, null, null, "NumeroExpediente", true, true, cancellationToken,
                e => e.Atenciones);

            var dtos = pagina.Data.ToDtoList();
            await dtos.EnriquecerAsync(_catalogos, cancellationToken);
            return dtos;
        }
    }

    public class ObtenerExpedientesPaginadosQuery : IRequest<PagedResult<ExpedienteDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Filter { get; set; }
        public string? OrderBy { get; set; }
    }

    public class ObtenerExpedientesPaginadosHandler
        : IRequestHandler<ObtenerExpedientesPaginadosQuery, PagedResult<ExpedienteDto>>
    {
        private readonly IGenericRepository<ExpedienteClinico> _repo;
        private readonly ICatalogosService _catalogos;

        public ObtenerExpedientesPaginadosHandler(
            IGenericRepository<ExpedienteClinico> repo, ICatalogosService catalogos)
        {
            _repo = repo;
            _catalogos = catalogos;
        }

        public async Task<PagedResult<ExpedienteDto>> Handle(
            ObtenerExpedientesPaginadosQuery request, CancellationToken cancellationToken)
        {
            var filtro = !string.IsNullOrWhiteSpace(request.Filter)
                ? Filter.FromStringExpression<ExpedienteClinico>(request.Filter)
                : null;

            var orden = string.IsNullOrWhiteSpace(request.OrderBy)
                ? "NumeroExpediente"
                : request.OrderBy;

            var pagina = await _repo.GetPagedAsync(
                request.PageNumber, request.PageSize, filtro, null, orden, true, true, cancellationToken,
                e => e.Atenciones);

            var dtos = pagina.Data.ToDtoList();
            await dtos.EnriquecerAsync(_catalogos, cancellationToken);

            return new PagedResult<ExpedienteDto>
            {
                Data = dtos,
                TotalRecords = pagina.TotalRecords,
                PageSize = pagina.PageSize,
                CurrentPage = pagina.CurrentPage
            };
        }
    }

    public class ObtenerExpedientePorIdQuery : IRequest<ExpedienteDto>
    {
        public long ExpedienteId { get; set; }
    }

    public class ObtenerExpedientePorIdHandler
        : IRequestHandler<ObtenerExpedientePorIdQuery, ExpedienteDto>
    {
        private readonly IGenericRepository<ExpedienteClinico> _repo;
        private readonly ICatalogosService _catalogos;

        public ObtenerExpedientePorIdHandler(
            IGenericRepository<ExpedienteClinico> repo, ICatalogosService catalogos)
        {
            _repo = repo;
            _catalogos = catalogos;
        }

        public async Task<ExpedienteDto> Handle(
            ObtenerExpedientePorIdQuery request, CancellationToken cancellationToken)
        {
            var expedienteId = request.ExpedienteId;

            var pagina = await _repo.GetPagedAsync(
                1, 1, e => e.ExpedienteID == expedienteId, null, "ExpedienteID", true, true, cancellationToken,
                e => e.Atenciones);

            var expediente = pagina.Data.FirstOrDefault();

            if (expediente is null)
            {
                throw new DomainException(
                    Errores.EXPEDIENTE_NO_ENCONTRADO,
                    $"No existe un expediente con el Id {expedienteId}.");
            }

            var dto = expediente.ToDto();

            var lista = new List<ExpedienteDto> { dto };
            await lista.EnriquecerAsync(_catalogos, cancellationToken);

            return dto;
        }
    }

    /// <summary>El expediente de un paciente, buscado por su Id de paciente.</summary>
    public class ObtenerExpedientePorPacienteQuery : IRequest<ExpedienteDto>
    {
        public long PacienteId { get; set; }
    }

    public class ObtenerExpedientePorPacienteHandler
        : IRequestHandler<ObtenerExpedientePorPacienteQuery, ExpedienteDto>
    {
        private readonly IGenericRepository<ExpedienteClinico> _repo;
        private readonly ICatalogosService _catalogos;

        public ObtenerExpedientePorPacienteHandler(
            IGenericRepository<ExpedienteClinico> repo, ICatalogosService catalogos)
        {
            _repo = repo;
            _catalogos = catalogos;
        }

        public async Task<ExpedienteDto> Handle(
            ObtenerExpedientePorPacienteQuery request, CancellationToken cancellationToken)
        {
            var pacienteId = request.PacienteId;

            var pagina = await _repo.GetPagedAsync(
                1, 1, e => e.PacienteID == pacienteId, null, "ExpedienteID", true, true, cancellationToken,
                e => e.Atenciones);

            var expediente = pagina.Data.FirstOrDefault();

            if (expediente is null)
            {
                throw new DomainException(
                    Errores.EXPEDIENTE_NO_ENCONTRADO,
                    $"El paciente {pacienteId} todavía no tiene expediente; se le abre en su primera atención.");
            }

            var dto = expediente.ToDto();

            var lista = new List<ExpedienteDto> { dto };
            await lista.EnriquecerAsync(_catalogos, cancellationToken);

            return dto;
        }
    }
}
