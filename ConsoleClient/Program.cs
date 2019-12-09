using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace ConsoleClient
{
	public class Program
	{
		const string authority = "http://localhost:56311/";
		const string clientId = "console";
		const string clientSecret = "secret";
		const string scope = "book.read";
		const string apiUrl = "http://localhost:56706/api/books";

		static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

		public static async Task MainAsync()
		{
			Console.Title = "Console Client";

			var token = await GetTokenAsync();
			await CallApiAsync(token);

			Console.ReadLine();
		}

		private static async Task<string> GetTokenAsync()
		{
			var discoveryClient = new HttpClient();
			var discoveryResponse = await discoveryClient.GetDiscoveryDocumentAsync(authority);
			if (discoveryResponse.IsError)
			{
				Console.WriteLine($"Disco error: {discoveryResponse.Error}");
				return null;
			}

			var tokenClient = new HttpClient();
			var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
			{
				Address = discoveryResponse.TokenEndpoint,
				ClientId = clientId,
				ClientSecret = clientSecret,
				Scope = scope
			});
			if (tokenResponse.IsError)
			{
				Console.WriteLine($"Token endpoint error: {tokenResponse.Error}");
				return null;
			}

			Console.WriteLine($"Successfully obtaining an access token:\n{tokenResponse.AccessToken}\n");
			return tokenResponse.AccessToken;
		}

		private static async Task CallApiAsync(string token)
		{
			var client = new HttpClient();
			client.SetBearerToken(token);

			var response = await client.GetAsync(apiUrl);
			if (!response.IsSuccessStatusCode)
			{
				Console.WriteLine(response.StatusCode);
				Console.WriteLine(response);
			}
			else
			{
				var content = await response.Content.ReadAsStringAsync();
				Console.WriteLine("Successfully calling API:\n" + JArray.Parse(content));
			}
		}
	}
}
