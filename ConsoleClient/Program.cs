using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleClient
{
	public class Program
	{
		const string authority = "http://localhost:56311/";
		const string clientId = "console";
		const string clientSecret = "secret";
		const string scope = "api";
		const string apiUrl = "http://localhost:56706/api/id";

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
			var discoveryClient = new DiscoveryClient(authority);
			var discoveryResponse = await discoveryClient.GetAsync();
			if (discoveryResponse.IsError)
			{
				Console.WriteLine($"Disco error: {discoveryResponse.Error}");
				return null;
			}

			var tokenClient = new TokenClient(discoveryResponse.TokenEndpoint, clientId, clientSecret);
			var tokenResponse = await tokenClient.RequestClientCredentialsAsync(scope);
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
