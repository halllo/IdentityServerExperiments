using Microsoft.AspNetCore.Builder;
using MultiTenancy.Container;
using MultiTenancy.Resolution;

namespace MultiTenancy
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
			=> builder.UseMiddleware<MultiTenantContainerMiddleware<T>>();

		public static IApplicationBuilder UseMultiTenantContainer(this IApplicationBuilder builder)
			=> builder.UseMiddleware<MultiTenantContainerMiddleware<Tenant>>();
	}
}
