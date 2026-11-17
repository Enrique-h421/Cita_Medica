using Microsoft.Extensions.DependencyInjection;

namespace Catalogos.Core
{
    public static class Extension
    {
        /// <summary>Registra MediatR con todos los handlers de este proyecto.</summary>
        public static IServiceCollection AddCatalogosCore(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Extension).Assembly));
            return services;
        }
    }
}
