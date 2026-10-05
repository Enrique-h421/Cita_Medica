namespace Core.Services
{
    /// <summary>Llama al microservicio de Pagos para cobrar un pedido y devuelve
    /// el identificador de la transacción.</summary>
    public interface IPagoService
    {
        Task<string> ProcesarPagoAsync(Guid pedidoId, decimal monto, string moneda, CancellationToken cancellationToken = default);
    }
}