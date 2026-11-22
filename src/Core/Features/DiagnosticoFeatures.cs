using System.Linq.Expressions;
using Core.Dtos;
using Core.Exceptions;
using Core.Mappings;
using Domain.AgendaYAtencion;
using Domain.Diagnosticos;
using GenericPersistence.Repository;
using MediatR;

namespace Core.Features.Diagnosticos
{
    // ======================= Catálogo =======================

    /// <summary>
    /// Busca en el catálogo CIE-10 por código o por nombre. Es lo que alimenta
    /// el buscador de la pantalla de atención: el médico escribe "faring" y le
    /// salen las faringitis.
    ///
    /// Solo lectura. El catálogo lo siembra la clínica por SQL
    /// (sql/05_Diagnosticos.sql); no se edita desde la pantalla de consulta.
    /// </summary>
    public class BuscarDiagnosticosQuery : IRequest<List<DiagnosticoDto>>
    {
        public string? Busqueda { get; set; }
        public int Tope { get; set; } = 20;
    }

    public class BuscarDiagnosticosHandler
        : IRequestHandler<BuscarDiagnosticosQuery, List<DiagnosticoDto>>
    {
        private const int TopeMaximo = 100;

        private readonly IGenericRepository<Diagnostico> _repo;

        public BuscarDiagnosticosHandler(
            IGenericRepository<Diagnostico> repo) => _repo = repo;

        public async Task<List<DiagnosticoDto>> Handle(
            BuscarDiagnosticosQuery request, CancellationToken cancellationToken)
        {
            var texto = (request.Busqueda ?? string.Empty).Trim();

            var tope = request.Tope <= 0
                ? 20
                : Math.Min(request.Tope, TopeMaximo);

            // Los inactivos no se ofrecen: siguen existiendo para resolver el
            // nombre en expedientes viejos, pero ya no se pueden escoger.
            Expression<Func<Diagnostico, bool>> filtro = d => d.Activo;

            if (texto.Length > 0)
            {
                // Busca por nombre o por código, así el médico puede escribir
                // "faring" o directamente "J02".
                filtro = d => d.Activo
                    && (d.Nombre.Contains(texto)
                        || (d.CodigoCIE10 != null && d.CodigoCIE10.Contains(texto)));
            }

            var pagina = await _repo.GetPagedAsync(
                1, tope, filtro, null, "Nombre", true, false, cancellationToken);

            return pagina.Data.ToDtoList();
        }
    }

    // ======================= Diagnósticos de una atención =======================

    public class ObtenerDiagnosticosDeAtencionQuery : IRequest<List<AtencionDiagnosticoDto>>
    {
        public long AtencionId { get; set; }
    }

    public class ObtenerDiagnosticosDeAtencionHandler
        : IRequestHandler<ObtenerDiagnosticosDeAtencionQuery, List<AtencionDiagnosticoDto>>
    {
        private const int TopePorAtencion = 100;

        private readonly IGenericRepository<Atencion> _atenciones;
        private readonly IGenericRepository<AtencionDiagnostico> _vinculos;

        public ObtenerDiagnosticosDeAtencionHandler(
            IGenericRepository<Atencion> atenciones,
            IGenericRepository<AtencionDiagnostico> vinculos)
        {
            _atenciones = atenciones;
            _vinculos = vinculos;
        }

        public async Task<List<AtencionDiagnosticoDto>> Handle(
            ObtenerDiagnosticosDeAtencionQuery request, CancellationToken cancellationToken)
        {
            // Se valida que la atención exista para poder responder 404 en vez de
            // una lista vacía: no es lo mismo "no tiene diagnósticos" que
            // "esa consulta no existe".
            var atencion = await _atenciones.ObtenerPorIdAsync(request.AtencionId);

            if (atencion is null)
            {
                throw new DomainException(
                    Errores.ATENCION_NO_ENCONTRADA,
                    $"No existe una atención con el Id {request.AtencionId}.");
            }

            var atencionId = request.AtencionId;

            var pagina = await _vinculos.GetPagedAsync(
                1, TopePorAtencion,
                ad => ad.AtencionID == atencionId,
                null, "AtencionDiagnosticoID", true, false, cancellationToken,
                ad => ad.Diagnostico!);

            return pagina.Data.ToDtoList();
        }
    }

    /// <summary>
    /// Le agrega un diagnóstico a la consulta abierta. Es el acto clínico que
    /// cierra el ciclo: se agenda, se atiende, se diagnostica y se cierra.
    /// </summary>
    public class AgregarDiagnosticoCommand : IRequest<AtencionDiagnosticoDto>
    {
        public long AtencionId { get; set; }
        public AgregarDiagnosticoDto Datos { get; set; } = new();
    }

    public class AgregarDiagnosticoHandler
        : IRequestHandler<AgregarDiagnosticoCommand, AtencionDiagnosticoDto>
    {
        private const int TopePorAtencion = 100;

        private readonly IGenericRepository<Atencion> _atenciones;
        private readonly IGenericRepository<AtencionDiagnostico> _vinculos;
        private readonly IGenericRepository<Diagnostico> _catalogo;

        public AgregarDiagnosticoHandler(
            IGenericRepository<Atencion> atenciones,
            IGenericRepository<AtencionDiagnostico> vinculos,
            IGenericRepository<Diagnostico> catalogo)
        {
            _atenciones = atenciones;
            _vinculos = vinculos;
            _catalogo = catalogo;
        }

        public async Task<AtencionDiagnosticoDto> Handle(
            AgregarDiagnosticoCommand request, CancellationToken cancellationToken)
        {
            var atencion = await _atenciones.ObtenerPorIdAsync(request.AtencionId);

            if (atencion is null)
            {
                throw new DomainException(
                    Errores.ATENCION_NO_ENCONTRADA,
                    $"No existe una atención con el Id {request.AtencionId}.");
            }

            // Una atención cerrada es un documento firmado. Si el médico se
            // equivocó en el diagnóstico, se anula la atención y se documenta de
            // nuevo; no se le reescribe el contenido por detrás.
            if (atencion.Estado != EstadosAtencion.Abierta)
            {
                throw new DomainException(
                    Errores.ATENCION_YA_CERRADA,
                    $"La atención {atencion.AtencionID} no está abierta; ya no se le pueden agregar diagnósticos.");
            }

            var diagnostico = await _catalogo.ObtenerPorIdAsync(request.Datos.DiagnosticoId);

            if (diagnostico is null)
            {
                throw new DomainException(
                    Errores.DIAGNOSTICO_NO_ENCONTRADO,
                    $"No existe un diagnóstico con el Id {request.Datos.DiagnosticoId} en el catálogo.");
            }

            if (!diagnostico.Activo)
            {
                throw new DomainException(
                    Errores.DIAGNOSTICO_INVALIDO,
                    $"El diagnóstico \"{diagnostico.Nombre}\" está dado de baja en el catálogo y ya no se puede usar.");
            }

            var atencionId = request.AtencionId;

            var existentes = (await _vinculos.GetPagedAsync(
                1, TopePorAtencion,
                ad => ad.AtencionID == atencionId,
                null, "AtencionDiagnosticoID", true, false, cancellationToken)).Data.ToList();

            // El mismo código dos veces en la misma consulta no aporta nada y
            // ensucia el expediente.
            if (existentes.Any(ad => ad.DiagnosticoID == diagnostico.DiagnosticoID))
            {
                throw new DomainException(
                    Errores.DIAGNOSTICO_DUPLICADO,
                    $"La atención {atencionId} ya tiene registrado el diagnóstico \"{diagnostico.Nombre}\".");
            }

            var tienePrincipal = existentes.Any(
                ad => ad.TipoDiagnostico == TiposDiagnostico.Principal);

            var tipo = ResolverTipo(request.Datos.TipoDiagnostico, tienePrincipal);

            // En codificación clínica el principal es uno: el que explica la
            // consulta. Lo demás va como secundario.
            if (tipo == TiposDiagnostico.Principal && tienePrincipal)
            {
                throw new DomainException(
                    Errores.DIAGNOSTICO_INVALIDO,
                    "La atención ya tiene un diagnóstico principal. Registre este como secundario, "
                    + "o quite el principal actual si fue un error.");
            }

            var vinculo = new AtencionDiagnostico
            {
                AtencionID = atencionId,
                DiagnosticoID = diagnostico.DiagnosticoID,
                TipoDiagnostico = tipo,
                Observaciones = request.Datos.Observaciones
            };

            await _vinculos.CrearAsync(vinculo);

            // El catálogo ya se consultó arriba al validar: se reusa en vez de
            // volver a leerlo solo para traer el nombre.
            vinculo.Diagnostico = diagnostico;

            return vinculo.ToDto();
        }

        /// <summary>
        /// Si el cliente no manda el tipo, se deduce: el primer diagnóstico de
        /// la consulta es el principal y los siguientes son secundarios. Así la
        /// pantalla puede dejar que el médico solo escoja el diagnóstico y no
        /// tenga que pensar en la codificación.
        /// </summary>
        private static string ResolverTipo(string? tipoPedido, bool yaTienePrincipal)
        {
            if (string.IsNullOrWhiteSpace(tipoPedido))
            {
                return yaTienePrincipal
                    ? TiposDiagnostico.Secundario
                    : TiposDiagnostico.Principal;
            }

            var tipo = TiposDiagnostico.Normalizar(tipoPedido);

            if (tipo is null)
            {
                throw new DomainException(
                    Errores.DIAGNOSTICO_INVALIDO,
                    $"El tipo de diagnóstico \"{tipoPedido}\" no es válido. Use "
                    + $"{TiposDiagnostico.Principal} o {TiposDiagnostico.Secundario}.");
            }

            return tipo;
        }
    }

    /// <summary>
    /// Quita un diagnóstico de la consulta. Esto sí borra la fila, a diferencia
    /// de la atención: el diagnóstico todavía no está firmado, estamos
    /// corrigiendo un borrador mientras el paciente sigue enfrente.
    /// </summary>
    public class QuitarDiagnosticoCommand : IRequest<bool>
    {
        public long AtencionId { get; set; }
        public long AtencionDiagnosticoId { get; set; }
    }

    public class QuitarDiagnosticoHandler : IRequestHandler<QuitarDiagnosticoCommand, bool>
    {
        private readonly IGenericRepository<Atencion> _atenciones;
        private readonly IGenericRepository<AtencionDiagnostico> _vinculos;

        public QuitarDiagnosticoHandler(
            IGenericRepository<Atencion> atenciones,
            IGenericRepository<AtencionDiagnostico> vinculos)
        {
            _atenciones = atenciones;
            _vinculos = vinculos;
        }

        public async Task<bool> Handle(
            QuitarDiagnosticoCommand request, CancellationToken cancellationToken)
        {
            var atencion = await _atenciones.ObtenerPorIdAsync(request.AtencionId);

            if (atencion is null)
            {
                throw new DomainException(
                    Errores.ATENCION_NO_ENCONTRADA,
                    $"No existe una atención con el Id {request.AtencionId}.");
            }

            if (atencion.Estado != EstadosAtencion.Abierta)
            {
                throw new DomainException(
                    Errores.ATENCION_YA_CERRADA,
                    $"La atención {atencion.AtencionID} no está abierta; sus diagnósticos ya no se pueden modificar.");
            }

            var vinculo = await _vinculos.ObtenerPorIdAsync(request.AtencionDiagnosticoId);

            if (vinculo is null)
            {
                throw new DomainException(
                    Errores.DIAGNOSTICO_NO_ENCONTRADO,
                    $"No existe el diagnóstico {request.AtencionDiagnosticoId} en ninguna atención.");
            }

            // Que el Id exista no basta: tiene que ser de esta atención. Si no se
            // validara, se podría borrar el diagnóstico de la consulta de otro
            // paciente pasando el Id correcto en la ruta equivocada.
            if (vinculo.AtencionID != request.AtencionId)
            {
                throw new DomainException(
                    Errores.DIAGNOSTICO_NO_ENCONTRADO,
                    $"El diagnóstico {request.AtencionDiagnosticoId} no pertenece a la atención {request.AtencionId}.");
            }

            await _vinculos.EliminarAsync(vinculo.AtencionDiagnosticoID);
            return true;
        }
    }
}
