using Core.Dtos;
using Core.Exceptions;
using Core.Mappings;
using Core.Services;
using Domain.AgendaYAtencion;
using Domain.Expedientes;
using GenericPersistence.FiltroDinamico;
using GenericPersistence.Repository;
using MediatR;

namespace Core.Features.Atenciones
{
    // ======================= Queries =======================

    public class ObtenerAtencionesQuery : IRequest<List<AtencionDto>> { }

    public class ObtenerAtencionesHandler
        : IRequestHandler<ObtenerAtencionesQuery, List<AtencionDto>>
    {
        private const int TopeListado = 1000;
        private const string OrdenPorDefecto = "FechaInicio descending";

        private readonly IGenericRepository<Atencion> _repo;
        private readonly ICatalogosService _catalogos;

        public ObtenerAtencionesHandler(IGenericRepository<Atencion> repo, ICatalogosService catalogos)
        {
            _repo = repo;
            _catalogos = catalogos;
        }

        public async Task<List<AtencionDto>> Handle(
            ObtenerAtencionesQuery request, CancellationToken cancellationToken)
        {
            // Se usa la versión paginada porque es la única que admite includes:
            // sin el expediente, el número de expediente vendría en null.
            var pagina = await _repo.GetPagedAsync(
                1, TopeListado, null, null, OrdenPorDefecto, true, false, cancellationToken,
                a => a.Expediente!);

            var dtos = pagina.Data.ToDtoList();
            await dtos.EnriquecerAsync(_catalogos, cancellationToken);
            return dtos;
        }
    }

    public class ObtenerAtencionPorIdQuery : IRequest<AtencionDto>
    {
        public long AtencionId { get; set; }
    }

    public class ObtenerAtencionPorIdHandler
        : IRequestHandler<ObtenerAtencionPorIdQuery, AtencionDto>
    {
        private readonly IGenericRepository<Atencion> _repo;
        private readonly IGenericRepository<ExpedienteClinico> _expedientes;
        private readonly ICatalogosService _catalogos;

        public ObtenerAtencionPorIdHandler(
            IGenericRepository<Atencion> repo,
            IGenericRepository<ExpedienteClinico> expedientes,
            ICatalogosService catalogos)
        {
            _repo = repo;
            _expedientes = expedientes;
            _catalogos = catalogos;
        }

        public async Task<AtencionDto> Handle(
            ObtenerAtencionPorIdQuery request, CancellationToken cancellationToken)
        {
            var atencion = await _repo.ObtenerPorIdAsync(request.AtencionId);

            if (atencion is null)
            {
                throw new DomainException(
                    Errores.ATENCION_NO_ENCONTRADA,
                    $"No existe una atención con el Id {request.AtencionId}.");
            }

            var dto = atencion.ToDto();

            var expediente = await _expedientes.ObtenerPorIdAsync(atencion.ExpedienteID);
            dto.NumeroExpediente = expediente?.NumeroExpediente;

            var lista = new List<AtencionDto> { dto };
            await lista.EnriquecerAsync(_catalogos, cancellationToken);

            return dto;
        }
    }

    public class ObtenerAtencionesPaginadasQuery : IRequest<PagedResult<AtencionDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Filter { get; set; }
        public string? OrderBy { get; set; }
    }

    public class ObtenerAtencionesPaginadasHandler
        : IRequestHandler<ObtenerAtencionesPaginadasQuery, PagedResult<AtencionDto>>
    {
        private readonly IGenericRepository<Atencion> _repo;
        private readonly ICatalogosService _catalogos;

        public ObtenerAtencionesPaginadasHandler(
            IGenericRepository<Atencion> repo, ICatalogosService catalogos)
        {
            _repo = repo;
            _catalogos = catalogos;
        }

        public async Task<PagedResult<AtencionDto>> Handle(
            ObtenerAtencionesPaginadasQuery request, CancellationToken cancellationToken)
        {
            var filtro = !string.IsNullOrWhiteSpace(request.Filter)
                ? Filter.FromStringExpression<Atencion>(request.Filter)
                : null;

            var orden = string.IsNullOrWhiteSpace(request.OrderBy)
                ? "FechaInicio descending"
                : request.OrderBy;

            var pagina = await _repo.GetPagedAsync(
                request.PageNumber, request.PageSize, filtro, null, orden, true, false, cancellationToken,
                a => a.Expediente!);

            var dtos = pagina.Data.ToDtoList();
            await dtos.EnriquecerAsync(_catalogos, cancellationToken);

            return new PagedResult<AtencionDto>
            {
                Data = dtos,
                TotalRecords = pagina.TotalRecords,
                PageSize = pagina.PageSize,
                CurrentPage = pagina.CurrentPage
            };
        }
    }

    /// <summary>El historial clínico de un paciente: todas sus atenciones.</summary>
    public class ObtenerAtencionesPorPacienteQuery : IRequest<List<AtencionDto>>
    {
        public long PacienteId { get; set; }
    }

    public class ObtenerAtencionesPorPacienteHandler
        : IRequestHandler<ObtenerAtencionesPorPacienteQuery, List<AtencionDto>>
    {
        private const int TopeHistorial = 500;

        private readonly IGenericRepository<Atencion> _repo;
        private readonly ICatalogosService _catalogos;

        public ObtenerAtencionesPorPacienteHandler(
            IGenericRepository<Atencion> repo, ICatalogosService catalogos)
        {
            _repo = repo;
            _catalogos = catalogos;
        }

        public async Task<List<AtencionDto>> Handle(
            ObtenerAtencionesPorPacienteQuery request, CancellationToken cancellationToken)
        {
            var pacienteId = request.PacienteId;

            var pagina = await _repo.GetPagedAsync(
                1, TopeHistorial,
                a => a.PacienteID == pacienteId,
                null, "FechaInicio descending", true, false, cancellationToken,
                a => a.Expediente!);

            var dtos = pagina.Data.ToDtoList();
            await dtos.EnriquecerAsync(_catalogos, cancellationToken);
            return dtos;
        }
    }

    // ======================= Commands =======================

    /// <summary>
    /// Abre una atención. Si el paciente no tiene expediente, se le abre uno
    /// con su número correlativo: así funciona en una clínica real, el
    /// expediente nace en la primera consulta.
    /// </summary>
    public class AbrirAtencionCommand : IRequest<AtencionDto>
    {
        public CrearAtencionDto Atencion { get; set; } = new();
    }

    public class AbrirAtencionHandler : IRequestHandler<AbrirAtencionCommand, AtencionDto>
    {
        private readonly IGenericRepository<Atencion> _repo;
        private readonly IGenericRepository<ExpedienteClinico> _expedientes;
        private readonly IGenericRepository<Cita> _citas;
        private readonly ICatalogosService _catalogos;

        public AbrirAtencionHandler(
            IGenericRepository<Atencion> repo,
            IGenericRepository<ExpedienteClinico> expedientes,
            IGenericRepository<Cita> citas,
            ICatalogosService catalogos)
        {
            _repo = repo;
            _expedientes = expedientes;
            _citas = citas;
            _catalogos = catalogos;
        }

        public async Task<AtencionDto> Handle(
            AbrirAtencionCommand request, CancellationToken cancellationToken)
        {
            var datos = request.Atencion;

            // Paciente y médico viven en Catálogos: no hay FK, la integridad se
            // valida preguntando, igual que al crear una cita.
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

            // Si viene de una cita, la cita tiene que existir y ser de ese paciente.
            if (datos.CitaId.HasValue)
            {
                var cita = await _citas.ObtenerPorIdAsync(datos.CitaId.Value);

                if (cita is null)
                {
                    throw new DomainException(
                        Errores.CITA_NO_ENCONTRADA,
                        $"No existe una cita con el Id {datos.CitaId.Value}.");
                }

                if (cita.PacienteID != datos.PacienteId)
                {
                    throw new DomainException(
                        Errores.ATENCION_INVALIDA,
                        $"La cita {datos.CitaId.Value} es de otro paciente; no se puede atender con este expediente.");
                }
            }

            // Un paciente no puede estar en dos consultas a la vez. Si quedó una
            // abierta es que nadie la cerró, y hay que cerrarla antes de seguir.
            var pacienteId = datos.PacienteId;

            var abierta = await _repo.GetOneByAsync(
                a => a.PacienteID == pacienteId && a.Estado == EstadosAtencion.Abierta);

            if (abierta is not null)
            {
                throw new DomainException(
                    Errores.ATENCION_INVALIDA,
                    $"El paciente ya tiene la atención {abierta.AtencionID} abierta. Ciérrela antes de abrir otra.");
            }

            var expediente = await ObtenerOAbrirExpedienteAsync(pacienteId);

            var atencion = datos.ToEntity();
            atencion.ExpedienteID = expediente.ExpedienteID;

            await _repo.CrearAsync(atencion);

            var dto = atencion.ToDto();
            dto.NumeroExpediente = expediente.NumeroExpediente;

            // Los nombres ya se consultaron arriba al validar: se reusan.
            dto.PacienteNombre = paciente.NombreCompleto;
            dto.MedicoNombre = medico.NombreCompleto;

            return dto;
        }

        /// <summary>
        /// Devuelve el expediente del paciente, abriéndolo si no lo tiene.
        /// El número sale del siguiente Id libre; si por borrados manuales ese
        /// número ya estuviera tomado, se corre hacia adelante hasta uno libre.
        /// </summary>
        private async Task<ExpedienteClinico> ObtenerOAbrirExpedienteAsync(long pacienteId)
        {
            var existente = await _expedientes.GetOneByAsync(x => x.PacienteID == pacienteId);

            if (existente is not null)
            {
                return existente;
            }

            var todos = (await _expedientes.ObtenerTodosAsync()).ToList();
            var siguiente = todos.Count > 0 ? todos.Max(x => x.ExpedienteID) + 1 : 1;

            string numero;

            do
            {
                numero = $"EXP-{siguiente:D6}";
                siguiente++;
            }
            while (todos.Any(x => x.NumeroExpediente == numero));

            var nuevo = new ExpedienteClinico
            {
                PacienteID = pacienteId,
                NumeroExpediente = numero,
                FechaApertura = DateTime.Today,
                Activo = true
            };

            await _expedientes.CrearAsync(nuevo);
            return nuevo;
        }
    }

    /// <summary>
    /// Cierra la atención con los hallazgos del médico y, si venía de una cita,
    /// deja esa cita en estado "Atendida". Ese es el ciclo completo: se agenda,
    /// se atiende, se cierra.
    /// </summary>
    public class CerrarAtencionCommand : IRequest<AtencionDto>
    {
        public long AtencionId { get; set; }
        public CerrarAtencionDto Cierre { get; set; } = new();
    }

    public class CerrarAtencionHandler : IRequestHandler<CerrarAtencionCommand, AtencionDto>
    {
        private readonly IGenericRepository<Atencion> _repo;
        private readonly IGenericRepository<ExpedienteClinico> _expedientes;
        private readonly IGenericRepository<Cita> _citas;
        private readonly IGenericRepository<EstadoCita> _estados;
        private readonly ICatalogosService _catalogos;

        public CerrarAtencionHandler(
            IGenericRepository<Atencion> repo,
            IGenericRepository<ExpedienteClinico> expedientes,
            IGenericRepository<Cita> citas,
            IGenericRepository<EstadoCita> estados,
            ICatalogosService catalogos)
        {
            _repo = repo;
            _expedientes = expedientes;
            _citas = citas;
            _estados = estados;
            _catalogos = catalogos;
        }

        public async Task<AtencionDto> Handle(
            CerrarAtencionCommand request, CancellationToken cancellationToken)
        {
            var atencion = await _repo.ObtenerPorIdAsync(request.AtencionId);

            if (atencion is null)
            {
                throw new DomainException(
                    Errores.ATENCION_NO_ENCONTRADA,
                    $"No existe una atención con el Id {request.AtencionId}.");
            }

            if (atencion.Estado == EstadosAtencion.Anulada)
            {
                throw new DomainException(
                    Errores.ATENCION_YA_ANULADA,
                    $"La atención {atencion.AtencionID} está anulada; no se puede cerrar.");
            }

            if (atencion.Estado != EstadosAtencion.Abierta)
            {
                throw new DomainException(
                    Errores.ATENCION_YA_CERRADA,
                    $"La atención {atencion.AtencionID} ya fue cerrada el {atencion.FechaFin:dd/MM/yyyy HH:mm}.");
            }

            var fechaFin = request.Cierre.FechaFin ?? DateTime.Now;

            // La base tiene el constraint CK_Atenciones_Fechas. Se valida aquí para
            // que el usuario reciba un mensaje claro y no un error de SQL Server.
            if (fechaFin < atencion.FechaInicio)
            {
                throw new DomainException(
                    Errores.ATENCION_INVALIDA,
                    "La hora de fin no puede ser anterior a la hora de inicio de la atención.");
            }

            request.Cierre.AplicarCierre(atencion, fechaFin);
            await _repo.ActualizarAsync(atencion);

            await MarcarCitaComoAtendidaAsync(atencion);

            var dto = atencion.ToDto();

            var expediente = await _expedientes.ObtenerPorIdAsync(atencion.ExpedienteID);
            dto.NumeroExpediente = expediente?.NumeroExpediente;

            var lista = new List<AtencionDto> { dto };
            await lista.EnriquecerAsync(_catalogos, cancellationToken);

            return dto;
        }

        /// <summary>
        /// Pasa la cita de origen a "Atendida". Si el estado no está sembrado en
        /// la tabla Estados_Cita, la cita se deja como estaba: cerrar el
        /// expediente clínico importa más que la etiqueta de la agenda, y no
        /// tiene sentido perder la documentación médica por un catálogo incompleto.
        /// </summary>
        private async Task MarcarCitaComoAtendidaAsync(Atencion atencion)
        {
            if (!atencion.CitaID.HasValue)
            {
                return;
            }

            var cita = await _citas.ObtenerPorIdAsync(atencion.CitaID.Value);

            if (cita is null)
            {
                return;
            }

            var atendida = await _estados.GetOneByAsync(
                e => e.Codigo == CodigosEstadoCita.Atendida);

            if (atendida is null || cita.EstadoCitaID == atendida.EstadoCitaID)
            {
                return;
            }

            cita.EstadoCitaID = atendida.EstadoCitaID;
            await _citas.ActualizarAsync(cita);
        }
    }

    /// <summary>Corrige lo documentado en una atención que sigue abierta.</summary>
    public class ActualizarAtencionCommand : IRequest<bool>
    {
        public long AtencionId { get; set; }
        public ActualizarAtencionDto Atencion { get; set; } = new();
    }

    public class ActualizarAtencionHandler : IRequestHandler<ActualizarAtencionCommand, bool>
    {
        private readonly IGenericRepository<Atencion> _repo;

        public ActualizarAtencionHandler(IGenericRepository<Atencion> repo) => _repo = repo;

        public async Task<bool> Handle(
            ActualizarAtencionCommand request, CancellationToken cancellationToken)
        {
            var atencion = await _repo.ObtenerPorIdAsync(request.AtencionId);

            if (atencion is null)
            {
                throw new DomainException(
                    Errores.ATENCION_NO_ENCONTRADA,
                    $"No existe una atención con el Id {request.AtencionId}.");
            }

            // Una atención cerrada es un documento firmado: ya no se edita.
            if (atencion.Estado != EstadosAtencion.Abierta)
            {
                throw new DomainException(
                    Errores.ATENCION_YA_CERRADA,
                    $"La atención {atencion.AtencionID} no está abierta; su contenido ya no se puede modificar.");
            }

            request.Atencion.AplicarCambios(atencion);
            await _repo.ActualizarAsync(atencion);
            return true;
        }
    }

    /// <summary>
    /// Anula la atención. No la borra: en una clínica un registro clínico no se
    /// elimina, se marca como anulado y queda la huella de que existió.
    /// </summary>
    public class AnularAtencionCommand : IRequest<bool>
    {
        public long AtencionId { get; set; }
    }

    public class AnularAtencionHandler : IRequestHandler<AnularAtencionCommand, bool>
    {
        private readonly IGenericRepository<Atencion> _repo;

        public AnularAtencionHandler(IGenericRepository<Atencion> repo) => _repo = repo;

        public async Task<bool> Handle(
            AnularAtencionCommand request, CancellationToken cancellationToken)
        {
            var atencion = await _repo.ObtenerPorIdAsync(request.AtencionId);

            if (atencion is null)
            {
                throw new DomainException(
                    Errores.ATENCION_NO_ENCONTRADA,
                    $"No existe una atención con el Id {request.AtencionId}.");
            }

            if (atencion.Estado == EstadosAtencion.Anulada)
            {
                throw new DomainException(
                    Errores.ATENCION_YA_ANULADA,
                    $"La atención {atencion.AtencionID} ya estaba anulada.");
            }

            atencion.Estado = EstadosAtencion.Anulada;
            await _repo.ActualizarAsync(atencion);
            return true;
        }
    }
}
