using Autofac.Multitenant;
using Microsoft.AspNetCore.Http;

namespace MultiTenancy.Resolution
{
    public class CookieResolutionStrategy : ITenantIdentificationStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieResolutionStrategy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool TryIdentifyTenant(out object tenantId)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                tenantId = null;
                return false;
            }
            else
            {
                const string stpTenantCookieKey = "devTenantOverrideCookieKey";
                if (_httpContextAccessor.HttpContext.Request.Cookies.ContainsKey(stpTenantCookieKey))
                {
                    var tenantCookie = _httpContextAccessor.HttpContext.Request.Cookies[stpTenantCookieKey];
                    tenantId = tenantCookie;
                    return true;
                }
                else
                {
                    tenantId = null;
                    return false;
                }
            }
        }
    }
}
