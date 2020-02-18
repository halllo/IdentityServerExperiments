using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenancy
{
	public interface ITenantResolutionStrategy
	{
		Task<string> GetTenantIdentifierAsync();
	}
}
