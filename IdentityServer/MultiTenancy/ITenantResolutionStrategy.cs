using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.MultiTenancy
{
	public interface ITenantResolutionStrategy
	{
		Task<string> GetTenantIdentifierAsync();
	}
}
