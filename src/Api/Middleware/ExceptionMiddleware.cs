using Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;

namespace Api.Middleware
{
    /// <summary>
    /// Captura las excepciones que suben desde los handlers y las traduce al sobre
    /// estándar HttpResponse. Una DomainException sale con su código del enum
    /// Errores; cualquier otra excepción sale como 500 sin filtrar detalles.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Error de negocio {Codigo} en {Ruta}", ex.Codigo, context.Request.Path);
                await EscribirRespuestaAsync(context, EstadoHttpPara(ex.Codigo), (int)ex.Codigo, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en {Ruta}", context.Request.Path);
                await EscribirRespuestaAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    500,
                    "Ocurrió un error inesperado en el servidor.");
            }
        }

        /// <summary>
        /// Un "no encontrado" es 404. Que otro microservicio esté caído es 503:
        /// no es culpa de quien hizo la petición y además se puede reintentar.
        /// El resto de errores de negocio son 400.
        ///
        /// PACIENTE_NO_ENCONTRADO y MEDICO_NO_ENCONTRADO quedan en 400 a
        /// propósito: el recurso que se pidió existe, lo que viene malo es el
        /// dato que mandó el cliente en el cuerpo.
        /// </summary>
        private static HttpStatusCode EstadoHttpPara(Errores codigo) => codigo switch
        {
            Errores.CITA_NO_ENCONTRADA => HttpStatusCode.NotFound,
            Errores.ATENCION_NO_ENCONTRADA => HttpStatusCode.NotFound,
            Errores.EXPEDIENTE_NO_ENCONTRADO => HttpStatusCode.NotFound,
            Errores.CONSULTORIO_NO_ENCONTRADO => HttpStatusCode.NotFound,
            Errores.CATALOGOS_NO_DISPONIBLE => HttpStatusCode.ServiceUnavailable,
            Errores.PASARELA_NO_DISPONIBLE => HttpStatusCode.ServiceUnavailable,
            Errores.ATENCION_YA_CERRADA => HttpStatusCode.Conflict,
            Errores.ATENCION_YA_ANULADA => HttpStatusCode.Conflict,

            // El diagnóstico que se pidió quitar no está en esa consulta: 404,
            // igual que cualquier recurso que no existe en esa ruta.
            Errores.DIAGNOSTICO_NO_ENCONTRADO => HttpStatusCode.NotFound,

            // Ya está registrado: la petición es válida, lo que choca es el
            // estado actual del recurso. Eso es 409, no 400.
            Errores.DIAGNOSTICO_DUPLICADO => HttpStatusCode.Conflict,

            _ => HttpStatusCode.BadRequest
        };

        private static async Task EscribirRespuestaAsync(
            HttpContext context,
            HttpStatusCode status,
            int errorCode,
            string mensaje)
        {
            // Si la respuesta ya empezó a escribirse no se puede reemplazar.
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            var sobre = new Core.Wrappers.HttpResponse<object>
            {
                Succeeded = false,
                Result = null,
                ErrorCode = errorCode,
                ErrorMessage = mensaje
            };

            var json = JsonConvert.SerializeObject(sobre, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            await context.Response.WriteAsync(json);
        }
    }
}
