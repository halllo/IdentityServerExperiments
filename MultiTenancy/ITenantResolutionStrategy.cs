using System.Threading.Tasks;

namespace MultiTenancy
{
	public interface ITenantResolutionStrategy
	{
		Task<string> GetTenantIdentifierAsync();
	}
}
