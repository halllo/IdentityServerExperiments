using System;
using Microsoft.Extensions.Options;
using MultiTenancy.Resolution;

namespace MultiTenancy.Options
{
	/// <summary>
	/// Tenant aware options cache
	/// </summary>
	/// <typeparam name="TOptions"></typeparam>
	/// <typeparam name="TTenant"></typeparam>
	internal class TenantOptionsCache<TOptions, TTenant> : IOptionsMonitorCache<TOptions>
		where TOptions : class
		where TTenant : Tenant
	{

		private readonly ITenantAccessor<TTenant> _tenantAccessor;
		private readonly TenantOptionsCacheDictionary<TOptions> _tenantSpecificOptionsCache =
			new TenantOptionsCacheDictionary<TOptions>();

		public TenantOptionsCache(ITenantAccessor<TTenant> tenantAccessor)
		{
			_tenantAccessor = tenantAccessor;
		}

		public void Clear()
		{
			_tenantSpecificOptionsCache.Get(_tenantAccessor.Tenant.Name).Clear();
		}

		public TOptions GetOrAdd(string name, Func<TOptions> createOptions)
		{
			var t = _tenantAccessor.Tenant;
			return _tenantSpecificOptionsCache.Get(t.Name).GetOrAdd(name, createOptions);
		}

		public bool TryAdd(string name, TOptions options)
		{
			return _tenantSpecificOptionsCache.Get(_tenantAccessor.Tenant.Name)
				.TryAdd(name, options);
		}

		public bool TryRemove(string name)
		{
			return _tenantSpecificOptionsCache.Get(_tenantAccessor.Tenant.Name)
				.TryRemove(name);
		}
	}
}
