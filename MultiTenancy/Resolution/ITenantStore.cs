using System.Threading.Tasks;

namespace MultiTenancy.Resolution
{
    public interface ITenantStore<T> where T : Tenant
    {
        Task<T> GetTenantAsync(string identifier);
    }
}
