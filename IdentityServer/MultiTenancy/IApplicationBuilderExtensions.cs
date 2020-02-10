using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.MultiTenancy
{
	/// <summary>
	/// Nice method to register our middleware
	/// </summary>
	public static class IApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseMultiTenancy<T>(this IApplicationBuilder builder) where T : Tenant
			=> builder.UseMiddleware<TenantMiddleware<T>>();

		public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder builder)
			=> builder.UseMiddleware<TenantMiddleware<Tenant>>();

		public static IApplicationBuilder UseMultiTenantContainer<T>(this IApplicationBuilder builder) where T : Tenant
		   => builder.UseMiddleware<MultitenantContainerMiddleware<T>>();

		public static IApplicationBuilder UseMultiTenantContainer(this IApplicationBuilder builder)
			=> builder.UseMiddleware<MultitenantContainerMiddleware<Tenant>>();
	}
}
