using System.Threading.Tasks;

namespace MultiTenancy.Resolution
{
	/// <summary>
	/// In memory store for testing
	/// </summary>
	public class InMemoryTenantStore : ITenantStore<Tenant>
	{
		public InMemoryTenantStore()
		{
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
