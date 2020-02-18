using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenancy
{
	public interface ITenantAccessor<T> where T : Tenant
	{
		T Tenant { get; }
	}
}
