using MediatR;
using Core.Exceptions;
using Core.Services;
using Core.Wrappers;

namespace Core.Features.Command
{
    public class CrearPedidoCommand : IRequest<HttpResponse<string>>
    {
        public Guid ClienteId { get; set; }
        public decimal Total { get; set; }
    }

    internal class CrearPedidoCommandHandler : IRequestHandler<CrearPedidoCommand, HttpResponse<string>>
    {
        private readonly IPagoService _pagoService;

        public CrearPedidoCommandHandler(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        public async Task<HttpResponse<string>> Handle(CrearPedidoCommand request, CancellationToken cancellationToken)
        {
            var pedidoId = Guid.NewGuid();
            // ...validar existencias, guardar el pedido, etc.

            string transaccionId;
            try
            {
                transaccionId = await _pagoService.ProcesarPagoAsync(pedidoId, request.Total, "USD", cancellationToken);
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                throw new DomainException(Errores.PAGO_FALLIDO, $"No fue posible procesar el pago: {ex.Message}");
            }

            return new HttpResponse<string> { Succeeded = true, Result = transaccionId };
        }
    }
}