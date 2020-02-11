using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace IdentityServer.MultiTenancy
{
	public class MultiTenantCookiePolicyMiddleware
	{
		CookiePolicyMiddleware inner;

		public MultiTenantCookiePolicyMiddleware(RequestDelegate next, IOptions<CookiePolicyOptions> options)
		{
			inner = new CookiePolicyMiddleware(next, options);
		}

		public Task Invoke(HttpContext context)
		{
			/* Middlewares requiring IOptions in the constructor cannot be tenant specific!
			** Because  middleware constructors are invoked on application startup, before any tenant information is available, 
			** those middlewares have to be wrapped and the options need to be taken from the request instead.
			*/
			var options = context.RequestServices.GetRequiredService<IOptions<CookiePolicyOptions>>();
			inner.Options = options.Value;
			return inner.Invoke(context);
		}
	}
}
