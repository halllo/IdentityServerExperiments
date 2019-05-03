﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityServer
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables()
				.AddAzureKeyVault()
				;

			Configuration = builder.Build();
		}

		private IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(b =>
			{
				b.AddConfiguration(Configuration.GetSection("Logging"));
				b.AddDebug();
				b.AddConsole();
				b.AddAzureWebAppDiagnostics();
			});
			services.AddCors(options => options.AddPolicy("mycustomcorspolicy", b => b.WithOrigins("http://meinetollewebsite.de").AllowAnyMethod().AllowAnyHeader()));
			services.AddMvc();

			var serviceProvider = services.BuildServiceProvider();

			services.AddIdentityServer()
				.AddSigningCredentialFromKeyVault(Configuration, serviceProvider.GetService<ILogger<Startup>>())
				.AddInMemoryIdentityResources(IdentityConfig.GetIdentityResources())
				.AddInMemoryApiResources(IdentityConfig.GetApis())
				.AddInMemoryClients(IdentityConfig.GetClients())
				.AddTestUsers(IdentityConfig.GetTestUsers())
				;

			services.AddTransientDecorator<ICorsPolicyProvider, CorsPolicyProvider>();

			services.Configure<CookiePolicyOptions>(options =>
			{
				options.CheckConsentNeeded = context => { return true; };
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			app.UseDeveloperExceptionPage();

			app.UseIdentityServer();

			app.UseCors("mycustomcorspolicy");

			app.UseDefaultFiles();
			app.UseStaticFiles(); // Install IdentityServer UI: iex ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/IdentityServer/IdentityServer4.Quickstart.UI/release/get.ps1'))

			app.UseCookiePolicy();
			app.UseMvcWithDefaultRoute();
		}
	}
}
