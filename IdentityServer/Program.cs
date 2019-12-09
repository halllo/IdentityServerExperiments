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
			var builtConfig = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddCommandLine(args)
				.Build();

			return Host.CreateDefaultBuilder(args)
					.ConfigureLogging(logging =>
					{
						logging.AddConfiguration(builtConfig.GetSection("Logging"));
						logging.AddDebug();
						logging.AddConsole();
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
