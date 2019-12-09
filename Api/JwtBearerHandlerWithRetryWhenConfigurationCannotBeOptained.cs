using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Api
{
	/// <summary>
	/// When UserManagement.Identity is deployed, the first requests against its API try 
	/// to get the .well-known/openid-configuration of UserManagement.Identity itself. 
	/// But the ServiceFabric might not be ready to serve this configuration back.
	/// We therefore try 10 times to get it. If we could get the own configuration 
	/// without having to ask the network, that would be even better.
	/// </summary>
	public class JwtBearerHandlerWithRetryWhenConfigurationCannotBeOptained : JwtBearerHandler
	{
		private readonly ILoggerFactory loggerFactory;

		public JwtBearerHandlerWithRetryWhenConfigurationCannotBeOptained(IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder, ISystemClock clock) : base(options, loggerFactory, encoder, clock)
		{
			this.loggerFactory = loggerFactory;
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if (Options.ConfigurationManager != null && !(Options.ConfigurationManager is RetryingAdapter))
			{
				Options.ConfigurationManager = new RetryingAdapter(
					retries: 10,
					waitBeforeRetry: TimeSpan.FromSeconds(5),
					logger: loggerFactory.CreateLogger<JwtBearerHandlerWithRetryWhenConfigurationCannotBeOptained>(),
					configurationManager: Options.ConfigurationManager);
			}
			return await base.HandleAuthenticateAsync();
		}


		private class RetryingAdapter : IConfigurationManager<OpenIdConnectConfiguration>
		{
			private readonly int retries;
			private readonly TimeSpan waitBeforeRetry;
			private readonly ILogger logger;
			private readonly IConfigurationManager<OpenIdConnectConfiguration> configurationManager;

			public RetryingAdapter(int retries, TimeSpan waitBeforeRetry, ILogger logger, IConfigurationManager<OpenIdConnectConfiguration> configurationManager)
			{
				this.retries = retries;
				this.waitBeforeRetry = waitBeforeRetry;
				this.logger = logger;
				this.configurationManager = configurationManager;
			}
			public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
			{
				for (int attempt = 1; attempt <= this.retries; attempt++)
				{
					try
					{
						return await this.configurationManager.GetConfigurationAsync(cancel);
					}
					catch (Exception e) when (e.InnerException is IOException)
					{
						this.logger.LogError(e, $"Could not get OpenIdConnectConfiguration. Attempt {attempt} failed. Retrying in 5 seconds...");
						await Task.Delay(this.waitBeforeRetry);
						RequestRefresh();
						continue;
					}
				}
				return await this.configurationManager.GetConfigurationAsync(cancel);
			}

			public void RequestRefresh()
			{
				this.configurationManager.RequestRefresh();
			}
		}
	}
}
