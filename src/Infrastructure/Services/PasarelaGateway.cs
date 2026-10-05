using Core.Configs;
using Core.Exceptions;
using Core.Services;
using Domain.Models;
using NugetPackage_Rest.Exceptions;
using NugetPackage_Rest.Interfaces.IServices;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    internal class PasarelaGateway : IPasarelaGateway
    {
        private readonly IRest _rest;
        private readonly ITokenProvider _tokenProvider;
        private readonly PasarelaOptions _options;

        public PasarelaGateway(IRest rest, ITokenProvider tokenProvider, IOptions<PasarelaOptions> options)
        {
            _rest = rest;
            _tokenProvider = tokenProvider;
            _options = options.Value;
        }

        public async Task<CobroResponse> CobrarAsync(CobroRequest request, CancellationToken cancellationToken = default)
        {
            int totalIntentos = _options.MaxReintentos + 1;

            for (int intento = 1; intento <= totalIntentos; intento++)
            {
                try
                {
                    return await EnviarCobroAsync(request, cancellationToken);
                }
                catch (ApiException ex) when (EsReintentable(ex) && intento < totalIntentos)
                {
                    // Antes de reenviar, consultar si el cobro sí llegó a registrarse:
                    // reenviar un cobro ya aplicado cobraría dos veces al cliente.
                    var consulta = await ConsultarCobroAsync(request.Referencia, cancellationToken);
                    if (consulta.Estado == "APROBADO")
                        return consulta;
                }
            }

            throw new DomainException(Errores.PASARELA_NO_DISPONIBLE,
                $"No fue posible cobrar la referencia {request.Referencia} tras {_options.MaxReintentos} reintentos.");
        }

        public async Task<CobroResponse> ConsultarCobroAsync(string referencia, CancellationToken cancellationToken = default)
        {
            var token = await _tokenProvider.GetTokenAsync(cancellationToken);
            return await _rest.Get
                .WithBearer(token)
                .WithUri(_options.BaseUrl, $"/v1/cobros/{referencia}")
                .DeserializeWithAsync<CobroResponse>();
        }

        private async Task<CobroResponse> EnviarCobroAsync(CobroRequest request, CancellationToken cancellationToken)
        {
            var token = await _tokenProvider.GetTokenAsync(cancellationToken);
            return await _rest.Post
                .WithBearer(token)
                .WithUri(_options.BaseUrl, "/v1/cobros")
                .WithBody(request)
                .DeserializeWithAsync<CobroResponse>();
        }

        private static bool EsReintentable(ApiException ex) =>
            ex.Reason is ApiFailureReason.Timeout or ApiFailureReason.Network;
    }
}