using IdentityServer4;
using Microsoft.AspNetCore.Builder;
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
		private readonly IWebHostEnvironment env;
		private readonly IConfiguration config;

		public Startup(IWebHostEnvironment env, IConfiguration config)
		{
			this.env = env;
			this.config = config;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCors(options => options.AddPolicy("mycustomcorspolicy", b => b.WithOrigins("http://meinetollewebsite.de").AllowAnyMethod().AllowAnyHeader()));
			services.AddMvc();

			var serviceProvider = services.BuildServiceProvider();

			services.AddIdentityServer()
				.AddSigningCredentialFromKeyVault(config, serviceProvider.GetService<ILogger<Startup>>())
				.AddInMemoryIdentityResources(IdentityConfig.GetIdentityResources())
				.AddInMemoryApiResources(IdentityConfig.GetApis())
				.AddInMemoryClients(IdentityConfig.GetClients())
				.AddTestUsers(IdentityConfig.GetTestUsers())
				;

			services.AddAuthentication()
				.AddMicrosoftAccount("Microsoft", options =>
				{
					options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
					options.ClientId = config["MicrosoftAccountClientId"];
					options.ClientSecret = config["MicrosoftAccountClientSecret"];
					options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{config["MicrosoftAccountTenantId"]}/oauth2/v2.0/authorize";
					options.TokenEndpoint = $"https://login.microsoftonline.com/{config["MicrosoftAccountTenantId"]}/oauth2/v2.0/token";
				});

			services.AddTransientDecorator<ICorsPolicyProvider, CorsPolicyProvider>();

			services.Configure<CookiePolicyOptions>(options =>
			{
				options.CheckConsentNeeded = context => { return true; };
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseDeveloperExceptionPage();

			app.UseStaticFiles(); // Install IdentityServer UI: iex ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/IdentityServer/IdentityServer4.Quickstart.UI/release/get.ps1'))
			app.UseCookiePolicy();
			app.UseRouting();
			app.UseIdentityServer();
			app.UseCors("mycustomcorspolicy");//always after UseIdentityServer
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
			});
		}
	}
}
