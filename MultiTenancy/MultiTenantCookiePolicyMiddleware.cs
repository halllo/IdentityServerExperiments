using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MultiTenancy
{
	public class MultiTenantCookiePolicyMiddleware
	{
		private readonly RequestDelegate next;

		public MultiTenantCookiePolicyMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public Task Invoke(HttpContext context)
		{
			/* Middlewares requiring IOptions in the constructor cannot be tenant specific!
			** Because  middleware constructors are invoked on application startup, before any tenant information is available, 
			** those middlewares have to be wrapped and the options need to be taken from the request context instead.
			*/
			var options = context.RequestServices.GetRequiredService<IOptions<CookiePolicyOptions>>();
			var inner = new CookiePolicyMiddleware(next, options);
			return inner.Invoke(context);
		}
	}
}
