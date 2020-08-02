using Autofac.Multitenant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MultiTenancy.Container;

namespace MultiTenancy
{
    public static class ServicesExtensions
    {
        public static void AddMultiTenancy<TTenantIndetification>(this IServiceCollection services)
            where TTenantIndetification : class, ITenantIdentificationStrategy
        {
            services.AddAutofacMultitenantRequestServices();
            services.Insert(0, ServiceDescriptor.Transient<IStartupFilter, MultiTenantConfigureOnDemandStartupFilter>());

            services.TryAddSingleton<ITenantIdentificationStrategy, TTenantIndetification>();
        }
    }
}
