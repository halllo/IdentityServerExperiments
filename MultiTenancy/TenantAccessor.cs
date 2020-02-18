﻿using Microsoft.AspNetCore.Http;

namespace MultiTenancy
{
	public class TenantAccessor<T> : ITenantAccessor<T> where T : Tenant
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public TenantAccessor(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public T Tenant => _httpContextAccessor.HttpContext.GetTenant<T>();
	}
}
