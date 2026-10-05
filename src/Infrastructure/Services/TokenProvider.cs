using Core.Configs;
using Core.Exceptions;
using Core.Services;
using Domain.Models;
using NugetPackage_Rest.Interfaces.IServices;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    /// <summary>Obtiene un token OAuth 2.0 (client credentials) de la pasarela y lo
    /// guarda en memoria hasta un minuto antes de que expire. Se registra como
    /// Singleton para que la caché sobreviva entre requests.</summary>
    internal class TokenProvider : ITokenProvider
    {
        private readonly IRest _rest;
        private readonly PasarelaOptions _options;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private string? _token;
        private DateTime _expiraUtc;

        public TokenProvider(IRest rest, IOptions<PasarelaOptions> options)
        {
            _rest = rest;
            _options = options.Value;
        }

        public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            if (_token != null && DateTime.UtcNow < _expiraUtc) return _token;

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_token != null && DateTime.UtcNow < _expiraUtc) return _token;

                var response = await _rest.Post
                    .WithoutAuth()
                    .WithUri(_options.BaseUrl, "/oauth/token")
                    .WithFormUrlEncoded(new Dictionary<string, string>
                    {
                        ["grant_type"] = "client_credentials",
                        ["client_id"] = _options.ClientId,
                        ["client_secret"] = _options.ClientSecret
                    })
                    .DeserializeWithAsync<TokenResponse>();

                if (string.IsNullOrWhiteSpace(response?.AccessToken))
                    throw new DomainException(Errores.AUTENTICACION_FALLIDA, "La pasarela no devolvió un token de acceso.");

                _token = response.AccessToken;
                _expiraUtc = DateTime.UtcNow.AddSeconds(response.ExpiresIn - 60);
                return _token;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}