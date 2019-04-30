using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer
{
	public static class IdentityConfig
	{
		#region Scopes = Resources (Identity Resources und API Resources)
		public static IEnumerable<IdentityResource> GetIdentityResources()
		{
			return new IdentityResource[]
			{
				new IdentityResources.OpenId(),
				new IdentityResources.Profile()
			};
		}

		public static IEnumerable<ApiResource> GetApis()
		{
			return new ApiResource[]
			{
				new ApiResource("books-api", "Books API")
				{
					Scopes = new List<Scope>
					{
						new Scope("book.read", "Reading Books"),
						new Scope("book.write", "Writing Books"),
					}
				}
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
					AllowedScopes = { "book.read" }
				},
				new Client
				{
					ClientId = "angularclient",
					ClientName = "Angular Client",
					AllowedGrantTypes = GrantTypes.Implicit,
					AccessTokenLifetime = (int)TimeSpan.FromMinutes(5).TotalSeconds,
					AllowAccessTokensViaBrowser = true,
					RedirectUris = new List<string>
					{
						"http://localhost:4200/",
						"http://localhost:4200/assets/pages/silent-token-refresh.html",
						"https://localhost:4200/",
						"https://localhost:4200/assets/pages/silent-token-refresh.html",
						"https://manuels-angularclient.azurewebsites.net/",
						"https://manuels-angularclient.azurewebsites.net/assets/pages/silent-token-refresh.html"
					},
					PostLogoutRedirectUris = new List<string>
					{
						"http://localhost:4200/",
						"https://localhost:4200/",
						"https://manuels-angularclient.azurewebsites.net/",
					},
					AllowedScopes = { "openid", "profile", "book.read", "book.write" },
					AlwaysIncludeUserClaimsInIdToken = true
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
