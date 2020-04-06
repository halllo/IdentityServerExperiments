using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MultiTenancy.Resolution
{
	public class CookieResolutionStrategy : ITenantResolutionStrategy
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public CookieResolutionStrategy(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<string> GetTenantIdentifierAsync()
		{
			if (_httpContextAccessor.HttpContext == null)
			{
				return null;
			}
			else
			{
				const string stpTenantCookieKey = "STP-Tenant";
				if (_httpContextAccessor.HttpContext.Request.Cookies.ContainsKey(stpTenantCookieKey))
				{
					var tenantCookie = _httpContextAccessor.HttpContext.Request.Cookies[stpTenantCookieKey];
					return tenantCookie;
				}
				else
				{
					return null;
				}
			}
		}
	}
}
