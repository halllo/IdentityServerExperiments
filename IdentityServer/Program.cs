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
			return WebHost.CreateDefaultBuilder(args)
				.UseIISIntegration()
				.ConfigureServices(services =>
				{
					services.AddSingleton<IServiceProviderFactory<IServiceCollection>>(
						new ServiceProviderFactoryGenericAdapter<ContainerBuilder>(
							new MultiTenantServiceProviderFactory<Tenant>(Startup.ConfigureMultiTenantServices)
						)
					);
					services.AddMultiTenancy()
						.WithResolutionStrategy<SubdomainResolutionStrategy>()
						.WithStore<InMemoryTenantStore>(ServiceLifetime.Singleton)
						;
				})
				.UseStartup<Startup>();
		}

		public static IHostBuilder GenericHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new MultiTenantServiceProviderFactory<Tenant>(Startup.ConfigureMultiTenantServices))
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
