using Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;

namespace Api.Middleware
{
    /// <summary>
    /// Captura las excepciones que suben desde los handlers y las traduce al sobre
    /// estándar HttpResponse. Una DomainException sale como 400 con su código del
    /// enum Errores; cualquier otra excepción sale como 500 sin filtrar detalles.
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
                await EscribirRespuestaAsync(context, HttpStatusCode.BadRequest, (int)ex.Codigo, ex.Message);
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
