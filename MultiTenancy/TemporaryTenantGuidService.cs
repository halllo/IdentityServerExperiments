using System;

namespace MultiTenancy
{
	public class TemporaryTenantGuidService
	{
		public readonly Guid Id;

		public TemporaryTenantGuidService()
		{
			Id = Guid.NewGuid();
		}
	}
}
