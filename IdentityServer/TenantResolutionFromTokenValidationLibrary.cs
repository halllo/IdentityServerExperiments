using Autofac.Multitenant;
using Microsoft.AspNetCore.Http;
using MultiTenancy.Resolution;

namespace IdentityServer
{
    public class TenantResolutionFromTokenValidationLibrary : ITenantIdentificationStrategy
    {
        private readonly ITenantIdentificationStrategy resolutionStrategy;

        public TenantResolutionFromTokenValidationLibrary(IHttpContextAccessor httpContextAccessor)
        {
            this.resolutionStrategy = new CookieResolutionStrategy(httpContextAccessor);
        }

        public bool TryIdentifyTenant(out object tenantId)
        {
            return this.resolutionStrategy.TryIdentifyTenant(out tenantId);
        }
    }
}
