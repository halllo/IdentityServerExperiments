using Autofac;
using Autofac.Extensions.DependencyInjection;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiTenancy;
using MultiTenancy.Resolution;

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

			services.AddAuthentication();

			services.AddTransientDecorator<ICorsPolicyProvider, CorsPolicyProvider>();

			services.AddMultiTenancy()
				.WithResolutionStrategy<SubdomainResolutionStrategy>()
				.WithStore<InMemoryTenantStore>(ServiceLifetime.Singleton)
				;
		}

		public static void ConfigureMultiTenantServices(Tenant tenant, ContainerBuilder tenantContainer, IComponentContext applicationContainer)
		{
			var config = applicationContainer.Resolve<IConfiguration>();
			var services = new ServiceCollection();

			services.Configure<CookiePolicyOptions>(options =>
			{
				options.CheckConsentNeeded = context => { return true; };
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			services.AddSingleton<IResolvedTenant>(new ResolvedTenant(tenant));

			services.AddAuthenticationMinimumForTenant()
				.PrepareScheme<MicrosoftAccountOptions, MicrosoftAccountHandler>();


			if (tenant.Name == "123")
			{
				var authenticationBuilder = new AuthenticationBuilder(services);
				authenticationBuilder.AddMicrosoftAccount("Microsoft", options =>
				{
					options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
					options.ClientId = config["MicrosoftAccountClientId"];
					options.ClientSecret = config["MicrosoftAccountClientSecret"];
					options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{config["MicrosoftAccountTenantId"]}/oauth2/v2.0/authorize";
					options.TokenEndpoint = $"https://login.microsoftonline.com/{config["MicrosoftAccountTenantId"]}/oauth2/v2.0/token";
				});
			}
			else
			{
				services.Configure<AuthenticationOptions>(o =>
				{
					o.AddScheme(IISServerDefaults.AuthenticationScheme, scheme =>
					{
						scheme.HandlerType = typeof(IISServerAuthenticationHandler);
						scheme.DisplayName = IISServerDefaults.AuthenticationScheme + "!";
					});
				});
			}

			tenantContainer.Populate(services);
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseDeveloperExceptionPage();
			app.UseMultiTenancy().UseMultiTenantContainer();
			app.UseStaticFiles(); // Install IdentityServer UI: iex ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/IdentityServer/IdentityServer4.Quickstart.UI/release/get.ps1'))
			app.UseMiddleware<ScopedCookiePolicyMiddleware>();
			app.UseRouting();
			app.UseIdentityServer(new IdentityServerMiddlewareOptions { AuthenticationMiddleware = app => app.UseMiddleware<ScopedAuthenticationMiddleware>() });
			app.UseCors("mycustomcorspolicy");//always after UseIdentityServer
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
			});
		}
	}
}
