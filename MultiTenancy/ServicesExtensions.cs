using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MultiTenancy.Container;

namespace MultiTenancy
{
    public static class ServicesExtensions
    {
        public static void AddMultiTenancy(this IServiceCollection services)
        {
            services.AddAutofacMultitenantRequestServices();
            services.Insert(0, ServiceDescriptor.Transient<IStartupFilter, MultiTenantConfigureOnDemandStartupFilter>());
        }
    }
}
