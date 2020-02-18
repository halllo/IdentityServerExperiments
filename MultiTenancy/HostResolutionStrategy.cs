using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace MultiTenancy
{
	/// <summary>
	/// Resolve the host to a tenant identifier
	/// </summary>
	public class HostResolutionStrategy : ITenantResolutionStrategy
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IWebHostEnvironment environment;

		public HostResolutionStrategy(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
		{
			_httpContextAccessor = httpContextAccessor;
			this.environment = environment;
		}

		/// <summary>
		/// Get the tenant identifier
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public async Task<string> GetTenantIdentifierAsync()
		{
			if (_httpContextAccessor.HttpContext == null)
			{
				return null;
			}
			else
			{
				const string devTenantOverrideCookieKey = nameof(devTenantOverrideCookieKey);
				if (this.environment.IsDevelopment() && _httpContextAccessor.HttpContext.Request.Cookies.ContainsKey(devTenantOverrideCookieKey))
				{
					var tenantCookie = _httpContextAccessor.HttpContext.Request.Cookies[devTenantOverrideCookieKey];
					return tenantCookie;
				}
				else
				{
					var routeValues = _httpContextAccessor.HttpContext.Request.RouteValues;
					var path = _httpContextAccessor.HttpContext.Request.Path;
					var host = _httpContextAccessor.HttpContext.Request.Host;
					var hostHost = _httpContextAccessor.HttpContext.Request.Host.Host;

					string tenantIdentifier = host.ToString();
					return await Task.FromResult(tenantIdentifier);
				}
			}
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
	}
}
