using Domain.Models;

namespace Core.Services
{
    /// <summary>Realiza cobros contra la pasarela de pagos y permite consultar
    /// el estado de un cobro ya enviado.</summary>
    public interface IPasarelaGateway
    {
        Task<CobroResponse> CobrarAsync(CobroRequest request, CancellationToken cancellationToken = default);
        Task<CobroResponse> ConsultarCobroAsync(string referencia, CancellationToken cancellationToken = default);
    }
}