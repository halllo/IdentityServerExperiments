﻿using Microsoft.AspNetCore.Http;
using MultiTenancy.Resolution;

namespace MultiTenancy
{
	public static class HttpContextExtensions
	{
		/// <summary>
		/// Returns the current tenant
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="context"></param>
		/// <returns></returns>
		public static T GetTenant<T>(this HttpContext context) where T : Tenant
		{
			if (!context.Items.ContainsKey(MultiTenantConstants.HttpContextTenantKey))
				return null;
			return context.Items[MultiTenantConstants.HttpContextTenantKey] as T;
		}

		/// <summary>
		/// Returns the current Tenant
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static Tenant GetTenant(this HttpContext context)
		{
			return context.GetTenant<Tenant>();
		}
	}
}
