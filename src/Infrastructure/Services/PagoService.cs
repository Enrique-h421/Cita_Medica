using Core.Configs;
using Core.Services;
using Core.Wrappers;
using NugetPackage_Rest.Interfaces.IServices;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    internal class PagoService : IPagoService
    {
        private readonly IRest _rest;
        private readonly DownstreamOptions _options;

        public PagoService(IRest rest, IOptions<DownstreamOptions> options)
        {
            _rest = rest;
            _options = options.Value;
        }

        public async Task<string> ProcesarPagoAsync(Guid pedidoId, decimal monto, string moneda, CancellationToken cancellationToken = default)
        {
            var response = await _rest.Post
                .WithoutAuth()
                .WithUri(_options.PagosBaseUrl, "/api/Pagos/procesar")
                .WithBody(new { pedidoId, monto, moneda })
                .DeserializeWithAsync<HttpResponse<string>>();

            if (string.IsNullOrWhiteSpace(response?.Result))
                throw new InvalidOperationException(response?.ErrorMessage ?? "El servicio de Pagos no devolvió un identificador de transacción.");

            return response.Result;
        }
    }
}