using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
			services.AddIdentityServer()
				.AddDeveloperSigningCredential()
				.AddInMemoryApiResources(Config.GetApis())
				.AddInMemoryClients(Config.GetClients());
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseIdentityServer();

			app.Run(async (context) =>
			{
				await context.Response.WriteAsync("This is IdentityServer4!");
			});
		}
	}

	public static class Config
	{
		public static IEnumerable<ApiResource> GetApis()
		{
			return new ApiResource[]
			{
				new ApiResource("api", "Some API"),
				//new ApiResource
				//{
				//	Name = "complicated_api",
				//	DisplayName = "Complicated API",
				//	UserClaims = { "name", "email"},
				//	Scopes =
				//	{
				//		new Scope("full_access")
				//		{
				//			UserClaims = { "role"}
				//		},
				//		new Scope("read_only")
				//	}
				//}
			};
		}

		public static IEnumerable<Client> GetClients()
		{
			return new Client[]
			{
				new Client
				{
					ClientId = "console",
					ClientName = "Console App",
					AllowedGrantTypes = GrantTypes.ClientCredentials,
					ClientSecrets = { new Secret("secret".Sha256()) },
					AllowedScopes = { "api" }
				}
			};
		}
	}
}
