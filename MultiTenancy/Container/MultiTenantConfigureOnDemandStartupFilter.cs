using System;
using Autofac.Multitenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace MultiTenancy.Container
{
    internal class MultiTenantConfigureOnDemandStartupFilter : IStartupFilter
    {
        private readonly ITenantIdentificationStrategy tenantIdentificationStrategy;
        private readonly MultitenantContainer multitenantContainer;

        public MultiTenantConfigureOnDemandStartupFilter(ITenantIdentificationStrategy tenantIdentificationStrategy, MultitenantContainer multitenantContainer)
        {
            this.tenantIdentificationStrategy = tenantIdentificationStrategy;
            this.multitenantContainer = multitenantContainer;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.Use(async (context, next) =>
                {
                    if (this.tenantIdentificationStrategy.TryIdentifyTenant(out object tenantId) && !string.IsNullOrWhiteSpace(tenantId?.ToString() ?? ""))
                    {
                        this.multitenantContainer.ConfigureOnDemand(tenantId.ToString());
                    }

                    await next();
                });
                next(builder);
            };
        }
    }
}
