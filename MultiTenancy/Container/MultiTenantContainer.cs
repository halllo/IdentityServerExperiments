using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Multitenant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenancy.Container
{
    public class MultiTenantContainer<TTenantIndetification> : IMultiTenantContainer
        where TTenantIndetification : ITenantIdentificationStrategy
    {
        private readonly Action<string, IServiceCollection, IContainer> configureTenant;
        private MultitenantContainer multitenantContainer;

        public MultiTenantContainer(Action<string, IServiceCollection> configureTenant) : this((tenant, services, applicationContainer) => configureTenant(tenant, services))
        {
        }
        public MultiTenantContainer(Action<string, IServiceCollection, IContainer> configureTenant)
        {
            this.configureTenant = configureTenant;
        }

        private void Initialize(MultitenantContainer multitenantContainer)
        {
            if (this.multitenantContainer != null) throw new InvalidOperationException($"{nameof(MultitenantContainer)} was already initialized.");

            this.multitenantContainer = multitenantContainer;
        }

        public AutofacServiceProvider NewTenantScope(string tenantName)
        {
            ConfigureOnDemand(tenantName);
            return new AutofacServiceProvider(this.multitenantContainer.GetTenantScope(tenantName).BeginLifetimeScope("non-request"));
        }

        public void ConfigureOnDemand(string tenantName)
        {
            if (!string.IsNullOrWhiteSpace(tenantName))
            {
                if (!this.multitenantContainer.TenantIsConfigured(tenantName))
                {
                    var services = new ServiceCollection();
                    this.configureTenant(tenantName, services, this.multitenantContainer.ApplicationContainer);

                    this.multitenantContainer.ReconfigureTenant(tenantName, builder =>
                    {
                        builder.Populate(services);
                    });
                }
            }
        }

        public IServiceProviderFactory<ContainerBuilder> Factory => new AutofacMultitenantServiceProviderFactory(
            container =>
            {
                var tenantIndentification = container.Resolve<ITenantIdentificationStrategy>();
                var mtc = new MultitenantContainer(tenantIndentification, container);
                Initialize(mtc);
                return mtc;
            },
            container =>
            {
                container.RegisterInstance(this).As<IMultiTenantContainer>();
                container.RegisterType<TTenantIndetification>().As<ITenantIdentificationStrategy>().SingleInstance();
            });
    }
}
