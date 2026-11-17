using Catalogos.Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;

namespace Catalogos.Api.Middleware
{
    /// <summary>
    /// Mismo middleware que en Agenda: traduce las excepciones al sobre
    /// estándar HttpResponse para que los dos microservicios fallen igual.
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

        private static HttpStatusCode EstadoHttpPara(Errores codigo) => codigo switch
        {
            Errores.PACIENTE_NO_ENCONTRADO => HttpStatusCode.NotFound,
            Errores.MEDICO_NO_ENCONTRADO => HttpStatusCode.NotFound,
            Errores.ESPECIALIDAD_NO_ENCONTRADA => HttpStatusCode.NotFound,
            _ => HttpStatusCode.BadRequest
        };

        private static async Task EscribirRespuestaAsync(
            HttpContext context,
            HttpStatusCode status,
            int errorCode,
            string mensaje)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            var sobre = new Catalogos.Core.Wrappers.HttpResponse<object>
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
