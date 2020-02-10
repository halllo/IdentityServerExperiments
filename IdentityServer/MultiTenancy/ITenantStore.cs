using System.Threading.Tasks;

namespace IdentityServer.MultiTenancy
{
    public interface ITenantStore<T> where T : Tenant
    {
        Task<T> GetTenantAsync(string identifier);
    }
}
