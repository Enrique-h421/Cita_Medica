using Core.Configs;
using Core.Exceptions;
using Core.Services;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NugetPackage_Rest.Exceptions;
using NugetPackage_Rest.Interfaces.IServices;

namespace Infrastructure.Services
{
    /// <summary>
    /// Implementa contra el microservicio de Catálogos el contrato que Core
    /// definió en términos de negocio. Aquí, y solo aquí, se sabe que esto
    /// viaja por HTTP: los fallos de transporte se traducen a DomainException
    /// antes de salir de esta clase.
    ///
    /// Desde que Catálogos pide autenticación, esta clase reenvía el token con
    /// el que entró la petición. Agenda no tiene credenciales propias ni un
    /// usuario de servicio: actúa en nombre de quien la llamó. Eso es lo
    /// correcto — si recepción no puede ver el catálogo de pacientes, Agenda
    /// tampoco debe poder verlo por ella.
    /// </summary>
    internal class CatalogosService : ICatalogosService
    {
        private readonly IRest _rest;
        private readonly DownstreamOptions _options;
        private readonly IHttpContextAccessor _contexto;

        public CatalogosService(
            IRest rest,
            IOptions<DownstreamOptions> options,
            IHttpContextAccessor contexto)
        {
            _rest = rest;
            _options = options.Value;
            _contexto = contexto;
        }

        // ---------------- Consultas puntuales ----------------

        public Task<PacienteResumen?> ObtenerPacienteAsync(long pacienteId, CancellationToken cancellationToken = default)
            => ObtenerUnoAsync<PacienteResumen>($"/api/Pacientes/{pacienteId}", "pacientes");

        public Task<MedicoResumen?> ObtenerMedicoAsync(int medicoId, CancellationToken cancellationToken = default)
            => ObtenerUnoAsync<MedicoResumen>($"/api/Medicos/{medicoId}", "médicos");

        public Task<EspecialidadResumen?> ObtenerEspecialidadAsync(int especialidadId, CancellationToken cancellationToken = default)
            => ObtenerUnoAsync<EspecialidadResumen>($"/api/Especialidades/{especialidadId}", "especialidades");

        // ---------------- Listados tolerantes ----------------

        public Task<List<PacienteResumen>> ObtenerPacientesAsync(CancellationToken cancellationToken = default)
            => ObtenerListaAsync<PacienteResumen>("/api/Pacientes");

        public Task<List<MedicoResumen>> ObtenerMedicosAsync(CancellationToken cancellationToken = default)
            => ObtenerListaAsync<MedicoResumen>("/api/Medicos");

        public Task<List<EspecialidadResumen>> ObtenerEspecialidadesAsync(CancellationToken cancellationToken = default)
            => ObtenerListaAsync<EspecialidadResumen>("/api/Especialidades");

        // ---------------- Plomería ----------------

        /// <summary>
        /// Un 404 significa "no existe" y se devuelve como null, que es un dato
        /// válido para quien valida. Cualquier otro fallo significa "no se pudo
        /// averiguar", que es muy distinto: eso sube como DomainException para
        /// que nadie confunda un servicio caído con un registro inexistente.
        ///
        /// Un 401 cae en el segundo caso, y está bien que así sea: si el token
        /// no sirve, no se puede afirmar que el paciente no exista.
        /// </summary>
        private async Task<T?> ObtenerUnoAsync<T>(string endpoint, string catalogo) where T : class
        {
            var token = TokenEntrante();

            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return await _rest.Get
                        .WithoutAuth()
                        .WithUri(_options.CatalogosBaseUrl, endpoint)
                        .DeserializeWithAsync<T>();
                }

                return await _rest.Get
                    .WithBearer(token)
                    .WithUri(_options.CatalogosBaseUrl, endpoint)
                    .DeserializeWithAsync<T>();
            }
            catch (ApiException ex) when (ex.Reason == ApiFailureReason.HttpError && ex.StatusCode == 404)
            {
                return null;
            }
            catch (ApiException ex)
            {
                throw new DomainException(
                    Errores.CATALOGOS_NO_DISPONIBLE,
                    $"No se pudo consultar el catálogo de {catalogo}. El servicio de Catálogos respondió con un error ({ex.Reason}, HTTP {ex.StatusCode}).");
            }
        }

        /// <summary>
        /// Los listados son para decorar, no para decidir: si Catálogos no
        /// responde se devuelve lista vacía y la agenda se muestra sin nombres.
        /// </summary>
        private async Task<List<T>> ObtenerListaAsync<T>(string endpoint)
        {
            var token = TokenEntrante();

            try
            {
                List<T>? lista;

                if (string.IsNullOrWhiteSpace(token))
                {
                    lista = await _rest.Get
                        .WithoutAuth()
                        .WithUri(_options.CatalogosBaseUrl, endpoint)
                        .DeserializeWithAsync<List<T>>();
                }
                else
                {
                    lista = await _rest.Get
                        .WithBearer(token)
                        .WithUri(_options.CatalogosBaseUrl, endpoint)
                        .DeserializeWithAsync<List<T>>();
                }

                return lista ?? new List<T>();
            }
            catch (ApiException)
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// Saca el token de la cabecera Authorization de la petición en curso.
        /// Devuelve null si no hay petición HTTP (por ejemplo si esto se llamara
        /// desde una tarea en segundo plano) o si viene sin token; en ese caso
        /// la llamada sale sin autenticación y Catálogos la rechazará, que es
        /// exactamente lo que debe pasar.
        /// </summary>
        private string? TokenEntrante()
        {
            var cabecera = _contexto.HttpContext?.Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(cabecera))
            {
                return null;
            }

            const string prefijo = "Bearer ";

            // La cabecera llega como "Bearer eyJ...". Se le quita el prefijo
            // porque WithBearer lo vuelve a poner.
            return cabecera.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase)
                ? cabecera[prefijo.Length..].Trim()
                : cabecera.Trim();
        }
    }
}
