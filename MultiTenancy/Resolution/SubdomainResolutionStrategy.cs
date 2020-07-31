using Autofac.Multitenant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace MultiTenancy.Resolution
{
    /// <summary>
    /// Resolve the host to a tenant identifier
    /// </summary>
    public class SubdomainResolutionStrategy : ITenantIdentificationStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment environment;

        public SubdomainResolutionStrategy(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
        {
            _httpContextAccessor = httpContextAccessor;
            this.environment = environment;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/38549143/how-do-i-get-the-current-subdomain-within-net-core-middleware
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        private static string GetSubDomain(HttpContext httpContext)
        {
            var subDomain = string.Empty;

            var host = httpContext.Request.Host.Host;

            if (!string.IsNullOrWhiteSpace(host))
            {
                subDomain = host.Split('.')[0];
            }

            return subDomain.Trim().ToLower();
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
                const string devTenantOverrideCookieKey = nameof(devTenantOverrideCookieKey);
                if ((this.environment.IsDevelopment() || this.environment.IsEnvironment("Local")) && _httpContextAccessor.HttpContext.Request.Cookies.ContainsKey(devTenantOverrideCookieKey))
                {
                    var tenantCookie = _httpContextAccessor.HttpContext.Request.Cookies[devTenantOverrideCookieKey];
                    tenantId = tenantCookie;
                    return true;
                }
                else
                {
                    var subdomain = GetSubDomain(_httpContextAccessor.HttpContext);
                    tenantId = subdomain;
                    return true;
                }
            }
        }
    }
}
