using Autofac.Extensions.DependencyInjection;

namespace MultiTenancy.Container
{
    public interface IMultiTenantContainer
    {
        void ConfigureOnDemand(string tenantName);
        AutofacServiceProvider NewTenantScope(string tenantName);
    }
}
