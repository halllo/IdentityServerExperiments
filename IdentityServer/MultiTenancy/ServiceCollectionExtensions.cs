﻿using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.MultiTenancy
{
    /// <summary>
    /// Nice method to create the tenant builder
    /// </summary>
    public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Add the services (application specific tenant class)
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static TenantBuilder<T> AddMultiTenancy<T>(this IServiceCollection services) where T : Tenant
			=> new TenantBuilder<T>(services);

		/// <summary>
		/// Add the services (default tenant class)
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static TenantBuilder<Tenant> AddMultiTenancy(this IServiceCollection services)
			=> new TenantBuilder<Tenant>(services);
	}
}
