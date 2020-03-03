using Autofac;
using Autofac.Extensions.DependencyInjection;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
				.WithResolutionStrategy<HostResolutionStrategy>()
				.WithStore<InMemoryTenantStore>(ServiceLifetime.Singleton)
				;
		}

		public static void ConfigureMultiTenantServices(Tenant tenant, ContainerBuilder tenantContainer, IComponentContext applicationContainer)
		{
			var config = applicationContainer.Resolve<IConfiguration>();
			var services = new ServiceCollection();

			services.Configure<CookiePolicyOptions>(options =>
			{
				options.ConsentCookie.Name = options.ConsentCookie.Name + "-" + tenant.Id;
				options.CheckConsentNeeded = context => { return true; };
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			if (tenant.Identifier == "123")
			{
				services.AddAuthentication();
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

			tenantContainer.Populate(services);
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseDeveloperExceptionPage();

			app.UseMultiTenancy().UseMultiTenantContainer();

			app.UseStaticFiles(); // Install IdentityServer UI: iex ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/IdentityServer/IdentityServer4.Quickstart.UI/release/get.ps1'))

			app.UseMiddleware<ScopedCookiePolicyMiddleware>();

			app.Use(async (context, next) =>
			{
				await next();
			});
			app.UseRouting();// adds the IEndpointFeature to context.Features.
			app.Use(async (context, next) =>
			{
				await next();
			});
			app.UseIdentityServer(new IdentityServerMiddlewareOptions { AuthenticationMiddleware = app => app.UseMiddleware<ScopedAuthenticationMiddleware>() });
			app.UseCors("mycustomcorspolicy");//always after UseIdentityServer
			app.UseAuthorization();
			app.Use(async (context, next) =>
			{
				// for some reason, when AddMicrosoftAccount is registered in tenant container, the callback endpoint is not found (IEndpointFeature is not added) :(
				var endpoint = context.GetEndpoint();
				var endpointRouteBuilder = context.RequestServices.GetService<EndpointDataSource>();

				await next();
			});
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
			});
		}
	}
}
