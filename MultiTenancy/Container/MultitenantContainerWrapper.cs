using Autofac.Extensions.DependencyInjection;
using Autofac.Multitenant;

namespace MultiTenancy.Container
{
    internal class MultitenantContainerWrapper : IMultitenantContainer
    {
        private readonly MultitenantContainer mtc;

        public MultitenantContainerWrapper(MultitenantContainer mtc)
        {
            this.mtc = mtc;
        }

        public AutofacServiceProvider NewTenantScope(string tenantName)
        {
            return this.mtc.NewTenantScope(tenantName);
        }
    }
}
