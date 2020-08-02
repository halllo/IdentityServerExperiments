using System;
using Autofac;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiTenancy.Container;

namespace IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "IdentityServer";
            //GenericHostBuilder(args).Build().Run();
            WebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder WebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseIISIntegration()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IServiceProviderFactory<IServiceCollection>>(
                        new ServiceProviderFactoryGenericAdapter<ContainerBuilder>(
                            MultitenantContainerFactory.New(Startup.ConfigureMultiTenantServices)
                        )
                    );
                })
                .UseStartup<Startup>();
        }

        public static IHostBuilder GenericHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(MultitenantContainerFactory.New(Startup.ConfigureMultiTenantServices))
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
