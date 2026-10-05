namespace Core.Services
{
    /// <summary>Obtiene un token OAuth 2.0 (client credentials) de la pasarela y lo
    /// cachea en memoria hasta un minuto antes de que expire.</summary>
    public interface ITokenProvider
    {
        Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
    }
}