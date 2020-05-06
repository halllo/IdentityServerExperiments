using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace MultiTenancy.Container
{
	internal class MultiTenantContainerMiddleware<T> where T : Tenant
	{
		private readonly RequestDelegate next;

		public MultiTenantContainerMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task Invoke(HttpContext context, Func<MultiTenantContainer<T>> multiTenantContainerAccessor)
		{
			//Set to current tenant container.
			//Begin new scope for request as ASP.NET Core standard scope is per-request
			var autofacServiceProvider = new AutofacServiceProvider(multiTenantContainerAccessor().GetCurrentTenantScope().BeginLifetimeScope());
			var fe = context.Features.Select(f => f.Value).ToList(); //request services are handled by the RequestServicesFeature

			context.RequestServices = autofacServiceProvider;
			try
			{
				await next.Invoke(context);
			}
			finally
			{
				await autofacServiceProvider.DisposeAsync();
			}
		}
	}
}
