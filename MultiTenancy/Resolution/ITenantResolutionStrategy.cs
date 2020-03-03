using System.Threading.Tasks;

namespace MultiTenancy.Resolution
{
	public interface ITenantResolutionStrategy
	{
		Task<string> GetTenantIdentifierAsync();
	}
}
