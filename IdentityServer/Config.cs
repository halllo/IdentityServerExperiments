using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace IdentityServer
{
	public static class Config
	{
		#region Scopes = Resources (Identity Resources und API Resources)
		public static IEnumerable<IdentityResource> GetIdentityResources()
		{
			return new IdentityResource[]
			{
				new IdentityResources.OpenId(),
				new IdentityResources.Profile(),
			};
		}

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
		#endregion

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
				},
				new Client
				{
					ClientId = "spa",
					ClientName = "SPA App",
					AllowedGrantTypes = GrantTypes.Implicit,
					AllowAccessTokensViaBrowser = true,
					RedirectUris = new List<string>
					{
						"http://localhost:4200/"
					},
					PostLogoutRedirectUris = new List<string>
					{
						"http://localhost:4200/"
					},
					//LogoUri = "http://localhost:4200/",
					AllowedScopes = { "openid", "profile", "api" }
				},
			};
		}

		public static List<TestUser> GetTestUsers()
		{
			return new List<TestUser>
			{
				new TestUser
				{
					SubjectId = "123", Username = "bob", Password = "bob",
					Claims = new List<Claim>
					{
						new Claim("name", "Bob Loblaw"),
						new Claim("email", "Bob@Loblaw.com"),
						new Claim("location", "USA"),
					}
				}
			};
		}
	}
}
