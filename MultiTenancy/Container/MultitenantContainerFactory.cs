using System;
using Autofac;
using Autofac.Multitenant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenancy.Container
{
    public static class MultitenantContainerFactory
    {
        public static IServiceProviderFactory<ContainerBuilder> New(ConfigureTenant configureTenant, Action<ContainerBuilder> configurationAction = null)
        {
            return new AutofacMultitenantServiceProviderFactory(
                container =>
                {
                    var tenantIndentification = container.IsRegistered<ITenantIdentificationStrategy>()
                        ? container.Resolve<ITenantIdentificationStrategy>()
                        : new Resolution.ConstantResolutionStrategy();

                    /* 
                     * An ITenantIdentificationStrategy may not be registed, when WebHostBuilder is used.
                     * WebHostBuilder seems to create an ephemeral MultitenantContainer for internal stuff.
                     * The real MTC should be built after Startup.ConfigureServices was invoked and an ITenantIdentificationStrategy was registed.
                    **/

                    var mtc = new MultitenantContainer(tenantIndentification, container);
                    return mtc;
                },
                container =>
                {
                    container.RegisterInstance(configureTenant);
                    container.RegisterType<MultitenantContainerWrapper>().As<IMultitenantContainer>().SingleInstance();
                    configurationAction?.Invoke(container);
                });
        }
    }

    public delegate void ConfigureTenant(string tenant, IServiceCollection services, IContainer applicationContainer);
}
