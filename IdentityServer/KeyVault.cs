using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer
{
	public static class KeyVault
	{
		public static readonly Func<IConfiguration, string> Url = config => config["KeyVault"];
		public static readonly Func<IConfiguration, string> ClientId = config => config["KeyVaultClientId"];
		public static readonly Func<IConfiguration, string> ClientSecret = config => config["KeyVaultClientSecret"];

		public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder builder)
		{
			IConfiguration config = builder.Build();
			if (IsKeyVaultConfigured(config))
			{
				builder.AddAzureKeyVault(Url(config), ClientId(config), ClientSecret(config));
			}

			return builder;
		}

		public static IIdentityServerBuilder AddSigningCredentialFromKeyVault(this IIdentityServerBuilder builder, IConfiguration config, ILogger logger)
		{
			if (IsKeyVaultConfigured(config))
			{
				var cert = DownloadCertificateWithPrivateKey(config, logger, "tokensigningcertificate").Result;
				builder.AddSigningCredential(cert);
			}
			else
			{
				logger.LogCritical("No KeyVault configuration. Falling back to dev cert.");
				builder.AddDeveloperSigningCredential(persistKey: false);
			}

			return builder;
		}

		public static async Task<X509Certificate2> DownloadCertificateWithPrivateKey(IConfiguration config, ILogger logger, string certificateName)
		{
			try
			{
				using (var kvClient = new KeyVaultClient(GetToken))
				{
					var kvCertificate = await kvClient.GetCertificateAsync($"{Url(config).NoEndingSlash()}/certificates/{certificateName}");
					var kvCertificateSecret = await kvClient.GetSecretAsync(kvCertificate.SecretIdentifier.BaseIdentifier);

					var publicAndPrivateKey = Convert.FromBase64String(kvCertificateSecret.Value);
					var certificate = new X509Certificate2(publicAndPrivateKey, (string)null, X509KeyStorageFlags.MachineKeySet);
					return certificate;
				}
			}
			catch (Exception e)
			{
				logger.LogCritical("Cannot download cert from KeyVault.", e);
				throw;
			}

			async Task<string> GetToken(string authority, string resource, string scope)
			{
				var authContext = new AuthenticationContext(authority);
				var clientCred = new ClientCredential(ClientId(config), ClientSecret(config));
				var result = await authContext.AcquireTokenAsync(resource, clientCred);
				return result.AccessToken;
			}
		}

		private static bool IsKeyVaultConfigured(IConfiguration config)
		{
			return !string.IsNullOrWhiteSpace(Url(config))
				&& !string.IsNullOrWhiteSpace(ClientId(config))
				&& !string.IsNullOrWhiteSpace(ClientSecret(config));
		}
	}
}
