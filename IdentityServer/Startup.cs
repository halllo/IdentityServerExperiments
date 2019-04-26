using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(b =>
			{
				b.AddConfiguration(Configuration.GetSection("Logging"));
				b.AddDebug();
				b.AddConsole();
			});
			services.AddCors();
			services.AddMvc();

			services.AddIdentityServer()
				.AddDeveloperSigningCredential()
				.AddInMemoryIdentityResources(Config.GetIdentityResources())
				.AddInMemoryApiResources(Config.GetApis())
				.AddInMemoryClients(Config.GetClients())
				.AddTestUsers(Config.GetTestUsers())
				.AddCorsPolicyService<AllAllowedCorsPolicyService>()
				;

			services.Configure<CookiePolicyOptions>(options =>
			{
				options.CheckConsentNeeded = context => { return true; };
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseIdentityServer();

			app.UseDefaultFiles();
			app.UseStaticFiles(); // Install IdentityServer UI: iex ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/IdentityServer/IdentityServer4.Quickstart.UI/release/get.ps1'))

			app.UseCookiePolicy();
			app.UseMvcWithDefaultRoute();
		}
	}






	public class AllAllowedCorsPolicyService : ICorsPolicyService
	{
		public Task<bool> IsOriginAllowedAsync(string origin)
		{
			return Task.FromResult(true);
		}
	}
}
