﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.MultiTenancy
{
	internal class TenantMiddleware<T> where T : Tenant
	{
		private readonly RequestDelegate next;

		public TenantMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			if (!context.Items.ContainsKey(MultiTenantConstants.HttpContextTenantKey))
			{
				var tenantService = context.RequestServices.GetService(typeof(TenantAccessService<T>)) as TenantAccessService<T>;
				context.Items.Add(MultiTenantConstants.HttpContextTenantKey, await tenantService.GetTenantAsync());
			}

			//Continue processing
			if (next != null)
				await next(context);
		}
	}
}
