using System.Net;
using Microsoft.Extensions.Options;
using NugetPackage_Rest.Builders;
using NugetPackage_Rest.Configs;
using NugetPackage_Rest.Interfaces.IServices;
using Core.Configs;
using Infrastructure.Services;
using NugetPackage_Rest.Tests.Fakes;

namespace NugetPackage_Rest.Tests
{
    public class PagoServiceTests
    {
        [Fact]
        public async Task ProcesarPagoAsync_DevuelveIdentificadorDeTransaccion()
        {
            HttpRequestMessage? enviado = null;
            var handler = new FakeHttpMessageHandler(req =>
            {
                enviado = req;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"succeeded\":true,\"result\":\"TX-0001\"}")
                };
            });

            IRest rest = new RestBuilder(new HttpClient(handler), Options.Create(new RequestSettings()));
            var options = Options.Create(new DownstreamOptions { PagosBaseUrl = "http://pagos" });

            var service = new PagoService(rest, options);

            var transaccionId = await service.ProcesarPagoAsync(Guid.NewGuid(), 25.50m, "USD");

            Assert.Equal("TX-0001", transaccionId);
            Assert.Equal("http://pagos/api/Pagos/procesar", enviado!.RequestUri!.ToString());
        }
    }
}