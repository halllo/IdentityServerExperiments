using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityServer
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.Title = "IdentityServer";
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
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
						webBuilder.UseStartup<Startup>();
					});
		}
	}
}
