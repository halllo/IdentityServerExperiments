using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Multitenant;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenancy.Container
{
    internal static class MultitenantContainerExtensions
    {
        public static void ConfigureOnDemand(this MultitenantContainer mtc, string tenantName)
        {
            if (!string.IsNullOrWhiteSpace(tenantName))
            {
                if (!mtc.TenantIsConfigured(tenantName))
                {
                    var services = new ServiceCollection();

                    var configureTenant = mtc.ApplicationContainer.Resolve<ConfigureTenant>();
                    configureTenant(tenantName, services, mtc.ApplicationContainer);

                    mtc.ReconfigureTenant(tenantName, builder =>
                    {
                        builder.Populate(services);
                    });
                }
            }
        }

        public static AutofacServiceProvider NewTenantScope(this MultitenantContainer mtc, string tenantName)
        {
            mtc.ConfigureOnDemand(tenantName);
            return new AutofacServiceProvider(mtc.GetTenantScope(tenantName).BeginLifetimeScope("non-request"));
        }
    }
}
