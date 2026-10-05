using MediatR;
using Core.Exceptions;
using Core.Services;
using Core.Wrappers;
using Domain.Models;

namespace Core.Features.Command
{
    public class CobrarPedidoCommand : IRequest<HttpResponse<CobroResponse>>
    {
        public string Referencia { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Moneda { get; set; } = "USD";
    }

    internal class CobrarPedidoCommandHandler : IRequestHandler<CobrarPedidoCommand, HttpResponse<CobroResponse>>
    {
        private readonly IPasarelaGateway _pasarelaGateway;

        public CobrarPedidoCommandHandler(IPasarelaGateway pasarelaGateway)
        {
            _pasarelaGateway = pasarelaGateway;
        }

        public async Task<HttpResponse<CobroResponse>> Handle(CobrarPedidoCommand request, CancellationToken cancellationToken)
        {
            var cobroRequest = new CobroRequest
            {
                Referencia = request.Referencia,
                Monto = request.Monto,
                Moneda = request.Moneda
            };

            CobroResponse response;
            try
            {
                response = await _pasarelaGateway.CobrarAsync(cobroRequest, cancellationToken);
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                throw new DomainException(Errores.PAGO_FALLIDO, $"No fue posible procesar el cobro: {ex.Message}");
            }

            return new HttpResponse<CobroResponse> { Succeeded = true, Result = response };
        }
    }
}