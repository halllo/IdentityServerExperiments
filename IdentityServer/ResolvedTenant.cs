namespace IdentityServer
{
    public interface IResolvedTenant
    {
        string TenantName { get; }
    }

    public class ResolvedTenant : IResolvedTenant
    {
        public ResolvedTenant(string tenant)
        {
            this.TenantName = tenant;
        }

        public string TenantName { get; }
    }
}
