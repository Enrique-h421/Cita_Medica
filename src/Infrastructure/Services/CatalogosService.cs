using Core.Configs;
using Core.Exceptions;
using Core.Services;
using Domain.Models;
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
    /// </summary>
    internal class CatalogosService : ICatalogosService
    {
        private readonly IRest _rest;
        private readonly DownstreamOptions _options;

        public CatalogosService(IRest rest, IOptions<DownstreamOptions> options)
        {
            _rest = rest;
            _options = options.Value;
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
        /// </summary>
        private async Task<T?> ObtenerUnoAsync<T>(string endpoint, string catalogo) where T : class
        {
            try
            {
                return await _rest.Get
                    .WithoutAuth()
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
                    $"No se pudo consultar el catálogo de {catalogo}. El servicio de Catálogos no respondió ({ex.Reason}).");
            }
        }

        /// <summary>
        /// Los listados son para decorar, no para decidir: si Catálogos no
        /// responde se devuelve lista vacía y la agenda se muestra sin nombres.
        /// </summary>
        private async Task<List<T>> ObtenerListaAsync<T>(string endpoint)
        {
            try
            {
                var lista = await _rest.Get
                    .WithoutAuth()
                    .WithUri(_options.CatalogosBaseUrl, endpoint)
                    .DeserializeWithAsync<List<T>>();

                return lista ?? new List<T>();
            }
            catch (ApiException)
            {
                return new List<T>();
            }
        }
    }
}
