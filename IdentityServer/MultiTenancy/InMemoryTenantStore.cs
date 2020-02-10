using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.MultiTenancy
{
	/// <summary>
	/// In memory store for testing
	/// </summary>
	public class InMemoryTenantStore : ITenantStore<Tenant>
	{
		public InMemoryTenantStore()
		{
			System.Console.WriteLine("InMemoryTenantStore.ctor");
		}

		/// <summary>
		/// Get a tenant for a given identifier
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>
		public async Task<Tenant> GetTenantAsync(string identifier)
		{
			return await Task.FromResult(new Tenant { Id = identifier, Identifier = identifier });
		}
	}
}
