namespace MultiTenancy.Resolution
{
	public interface ITenantAccessor<T> where T : Tenant
	{
		T Tenant { get; }
	}
}
