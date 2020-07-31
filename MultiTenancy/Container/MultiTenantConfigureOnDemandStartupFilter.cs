using System;
using Autofac.Multitenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenancy.Container
{
    public class MultiTenantConfigureOnDemandStartupFilter : IStartupFilter
    {
        private readonly ITenantIdentificationStrategy tenantIdentificationStrategy;

        public MultiTenantConfigureOnDemandStartupFilter(ITenantIdentificationStrategy tenantIdentificationStrategy)
        {
            this.tenantIdentificationStrategy = tenantIdentificationStrategy;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.Use(async (context, next) =>
                {
                    var mtc = context.RequestServices.GetService<IMultiTenantContainer>();
                    if (this.tenantIdentificationStrategy.TryIdentifyTenant(out object tenantId) && !string.IsNullOrWhiteSpace(tenantId?.ToString() ?? ""))
                    {
                        mtc.ConfigureOnDemand(tenantId.ToString());
                    }

                    await next();
                });
                next(builder);
            };
        }
    }
}
