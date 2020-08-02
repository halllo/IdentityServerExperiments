using Autofac.Multitenant;

namespace MultiTenancy.Resolution
{
    public class ConstantResolutionStrategy : ITenantIdentificationStrategy
    {
        public string TenantId { get; set; }

        public bool TryIdentifyTenant(out object tenantId)
        {
            tenantId = TenantId;
            return tenantId != null;
        }
    }
}
