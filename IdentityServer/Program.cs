using System;
using Autofac;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiTenancy;
using MultiTenancy.Container;
using MultiTenancy.Resolution;

namespace IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "IdentityServer";
            GenericHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder WebHostBuilder(string[] args)
        {
            var mtc = new MultiTenantContainer<CookieResolutionStrategy>(Startup.ConfigureMultiTenantServices);

            return WebHost.CreateDefaultBuilder(args)
                .UseIISIntegration()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IServiceProviderFactory<IServiceCollection>>(
                        new ServiceProviderFactoryGenericAdapter<ContainerBuilder>(mtc.Factory)
                    );
                    services.AddMultiTenancy();
                })
                .UseStartup<Startup>();
        }

        public static IHostBuilder GenericHostBuilder(string[] args)
        {
            var mtc = new MultiTenantContainer<CookieResolutionStrategy>(Startup.ConfigureMultiTenantServices);

            return Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(mtc.Factory)
                .ConfigureLogging(logging =>
                {
                    logging.AddAzureWebAppDiagnostics();
                })
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder.AddAzureKeyVault();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.UseIISIntegration();
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
