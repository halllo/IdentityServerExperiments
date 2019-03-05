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
					options.Authority = "https://stp.stp-cloud-dev.azure.local/identity/";
					options.RequireHttpsMetadata = false;
					options.ApiName = "man-test-api"; // audience
				});

			services.AddAuthorization(options =>
			{
				options.AddPolicy("book.read", policy =>
				{
					policy.RequireAuthenticatedUser();
					policy.Requirements.Add(new HasScopeRequirement(
						scope: "man.book.read"
					));
				});
				options.AddPolicy("book.write", policy =>
				{
					policy.RequireAuthenticatedUser();
					policy.Requirements.Add(new HasScopeRequirement(
						scope: "man.book.write"
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
