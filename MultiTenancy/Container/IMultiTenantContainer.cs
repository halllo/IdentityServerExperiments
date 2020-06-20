using System;

namespace MultiTenancy.Container
{
    public interface IMultiTenantContainer
    {
        IServiceProvider GetTenantContainer(string tenantName);
    }
}
