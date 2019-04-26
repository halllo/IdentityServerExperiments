using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer
{
	public static class KeyVault
	{
		public static IIdentityServerBuilder AddSigningCredentialFromKeyVault(this IIdentityServerBuilder builder)
		{
			//TODO: detect key vault presence and fall back to developer cert
			builder.AddDeveloperSigningCredential(persistKey: false);
			return builder;
		}

		private static X509Certificate2 GetIdentityServerCertificate(IServiceCollection services)
		{
			var clientId = "TODO-get from config";
			var clientSecret = "TODO-get from config";
			var secretIdentifier = "TODO-get from config";

			var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(async (authority, resource, scope) =>
			{
				var authContext = new AuthenticationContext(authority);
				ClientCredential clientCreds = new ClientCredential(clientId, clientSecret);

				AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCreds);

				if (result == null)
				{
					throw new InvalidOperationException("Failed to obtain the JWT token");
				}

				return result.AccessToken;
			}));

			var pfxSecret = keyVaultClient.GetSecretAsync(secretIdentifier).Result;
			var pfxBytes = Convert.FromBase64String(pfxSecret.Value);
			var certificate = new X509Certificate2(pfxBytes);
			return certificate;
		}
	}
}
