using MultiTenancy;

namespace IdentityServer
{
	public interface IResolvedTenant
	{
		string TenantName { get; }
	}

	public class ResolvedTenant : IResolvedTenant
	{
		private readonly Tenant t;

		public ResolvedTenant(Tenant t)
		{
			this.t = t;
		}

		public string TenantName => t?.Name ?? string.Empty;
	}
}
