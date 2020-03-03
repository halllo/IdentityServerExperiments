using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenancy
{
	public class ScopedAuthenticationMiddleware
	{
		private readonly RequestDelegate next;

		public ScopedAuthenticationMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public Task Invoke(HttpContext context)
		{
			/* Middlewares requiring IOptions in the constructor cannot be tenant specific!
			** Because  middleware constructors are invoked on application startup, before any tenant information is available, 
			** those middlewares have to be wrapped and the options need to be taken from the request context instead.
			*/
			var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
			var inner = new AuthenticationMiddleware(next, schemes);
			return inner.Invoke(context);
		}
	}
}
