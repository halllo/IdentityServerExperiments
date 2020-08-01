using Autofac.Extensions.DependencyInjection;

namespace MultiTenancy.Container
{
    public interface IMultitenantContainer
    {
        AutofacServiceProvider NewTenantScope(string tenantName);
    }
}
