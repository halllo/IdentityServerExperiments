// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

// Inlined for Debugging. Taken from https://github.com/IdentityServer/IdentityServer4/blob/44651bea9b02c992902639b21205f433aad47d03/src/IdentityServer4/src/Hosting/CorsPolicyProvider.cs

using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer
{
	internal class CorsPolicyProvider : ICorsPolicyProvider
	{
		private readonly ILogger _logger;
		private readonly ICorsPolicyProvider _inner;
		private readonly IdentityServerOptions _options;
		private readonly IHttpContextAccessor _httpContext;

		public CorsPolicyProvider(
			ILogger<CorsPolicyProvider> logger,
			Decorator<ICorsPolicyProvider> inner,
			IdentityServerOptions options,
			IHttpContextAccessor httpContext)
		{
			_logger = logger;
			_inner = inner.Instance;
			_options = options;
			_httpContext = httpContext;
		}

		public async Task<CorsPolicy> GetPolicyAsync(HttpContext context, string policyName)
		{
			Debug.WriteLine(context.Request.Path);

			if (_options.Cors.CorsPolicyName == policyName)
			{
				var result = await ProcessAsync(context);
				return result;
			}
			else
			{
				var result = await _inner.GetPolicyAsync(context, policyName);
				return result;
			}
		}

		private async Task<CorsPolicy> ProcessAsync(HttpContext context)
		{
			var origin = context.Request.GetCorsOrigin();
			if (origin != null)
			{
				var path = context.Request.Path;
				if (IsPathAllowed(path))
				{
					_logger.LogDebug("CORS request made for path: {path} from origin: {origin}", path, origin);

					// manually resolving this from DI because this: 
					// https://github.com/aspnet/CORS/issues/105
					var corsPolicyService = _httpContext.HttpContext.RequestServices.GetRequiredService<ICorsPolicyService>();

					if (await corsPolicyService.IsOriginAllowedAsync(origin))
					{
						_logger.LogDebug("CorsPolicyService allowed origin: {origin}", origin);
						return Allow(origin);
					}
					else
					{
						_logger.LogWarning("CorsPolicyService did not allow origin: {origin}", origin);
					}
				}
				else
				{
					_logger.LogDebug("CORS request made for path: {path} from origin: {origin} but was ignored because path was not for an allowed IdentityServer CORS endpoint", path, origin);
				}
			}

			return null;
		}

		private CorsPolicy Allow(string origin)
		{
			var policyBuilder = new CorsPolicyBuilder()
				.WithOrigins(origin)
				.AllowAnyHeader()
				.AllowAnyMethod();

			if (_options.Cors.PreflightCacheDuration.HasValue)
			{
				policyBuilder.SetPreflightMaxAge(_options.Cors.PreflightCacheDuration.Value);
			}

			return policyBuilder.Build();
		}

		private bool IsPathAllowed(PathString path)
		{
			return _options.Cors.CorsPaths.Any(x => path == x);
		}
	}
}
