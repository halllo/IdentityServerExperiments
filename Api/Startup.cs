﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Api
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
			services.AddControllers();
			//services.AddRazorPages();
			services.AddCors();
			services.AddAuthentication("Bearer")
				.AddIdentityServerAuthentication(options =>
				{
					options.Authority = Configuration["Authority"];
					options.RequireHttpsMetadata = false;
					options.ApiName = Configuration["Audience"];
				});
			services.AddTransient<JwtBearerHandler, JwtBearerHandlerWithRetryWhenConfigurationCannotBeOptained>();
			services.AddAuthorization(options =>
			{
				options.AddPolicy("book.read", policy =>
				{
					policy.RequireAuthenticatedUser();
					policy.Requirements.Add(new HasScopeRequirement(
						scope: "book.read"
					));
				});
				options.AddPolicy("book.write", policy =>
				{
					policy.RequireAuthenticatedUser();
					policy.Requirements.Add(new HasScopeRequirement(
						scope: "book.write"
					));
				});
			});
			services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseAuthentication();
			app.UseRouting();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync("This is just an API!");
				});
			});
		}
	}
}
