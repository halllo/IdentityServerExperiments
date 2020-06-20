using Microsoft.AspNetCore.Http;

namespace MultiTenancy.Resolution
{
    public class TenantAccessor<T> : ITenantAccessor<T> where T : Tenant
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public T Tenant
        {
            get
            {
                if (_httpContextAccessor.HttpContext == null)
                {
                    return null;
                }
                else
                {
                    return _httpContextAccessor.HttpContext.GetTenant<T>();
                }
            }
        }
    }
}
