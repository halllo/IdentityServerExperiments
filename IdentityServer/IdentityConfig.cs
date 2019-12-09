﻿using System;
using System.Collections.Generic;
using IdentityServer4.Models;
using IdentityServer4.Quickstart.UI;
using IdentityServer4.Test;

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
					AllowedGrantTypes = GrantTypes.Code,
					AccessTokenLifetime = (int)TimeSpan.FromMinutes(2).TotalSeconds,
					AllowAccessTokensViaBrowser = true,
					RequirePkce = true,
					RequireClientSecret = false,
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
					AllowedCorsOrigins = new List<string>
					{
						"http://localhost:4200",
						"https://localhost:4200",
						"https://manuels-angularclient.azurewebsites.net"
					},
					AllowedScopes = { "openid", "profile", "book.read", "book.write" },
					AlwaysIncludeUserClaimsInIdToken = true
				},
			};
		}

		public static List<TestUser> GetTestUsers()
		{
			return TestUsers.Users;
		}
	}
}
