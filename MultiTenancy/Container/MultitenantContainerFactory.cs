using Autofac;
using Autofac.Multitenant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenancy.Container
{
    public static class MultitenantContainerFactory
    {
        public static IServiceProviderFactory<ContainerBuilder> New<TTenantIndetification>(ConfigureTenant configureTenant)
            where TTenantIndetification : ITenantIdentificationStrategy
        {
            return new AutofacMultitenantServiceProviderFactory(
                container =>
                {
                    var tenantIndentification = container.Resolve<ITenantIdentificationStrategy>();
                    var mtc = new MultitenantContainer(tenantIndentification, container);
                    return mtc;
                },
                container =>
                {
                    container.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
                    container.RegisterType<TTenantIndetification>().As<ITenantIdentificationStrategy>().SingleInstance();
                    container.RegisterInstance(configureTenant);
                    container.RegisterType<MultitenantContainerWrapper>().As<IMultitenantContainer>().SingleInstance();
                });
        }
    }

    public delegate void ConfigureTenant(string tenant, IServiceCollection services, IContainer applicationContainer);
}
