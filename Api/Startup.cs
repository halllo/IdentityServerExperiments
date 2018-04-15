using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Api
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();

			services.AddAuthentication("Bearer")
				.AddIdentityServerAuthentication(options =>
				{
					options.Authority = "http://localhost:56311/";
					options.RequireHttpsMetadata = false;
					options.ApiName = "api"; // audience
				});

			services.AddAuthorization(options =>
			{
				options.AddPolicy("policy1", policy =>
				{
					policy.RequireAuthenticatedUser();
					policy.Requirements.Add(new HasScopeRequirement(
						scope: "api",
						issuer: "http://localhost:56311"
					));
				});
			});
			services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseCors(builder =>
				builder.WithOrigins(
					"http://localhost:4200"
				).AllowAnyHeader().AllowAnyMethod()
			);

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseAuthentication();
			app.UseMvc();

			app.Run(async (context) =>
			{
				await context.Response.WriteAsync("This is just an API!");
			});
		}
	}
}
