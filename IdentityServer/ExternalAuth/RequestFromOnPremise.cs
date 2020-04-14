using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityServer.Quickstart.Account;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IdentityServer.ExternalAuth
{
	public class RequestFromOnPremise
	{
		private readonly ILogger<RequestFromOnPremise> logger;

		public RequestFromOnPremise(ILogger<RequestFromOnPremise> logger)
		{
			this.logger = logger;
		}
		public async Task<HttpResponseMessage> Do(string tenantName, HttpRequestMessage request)
		{
			var tenantId = tenantName;//optional mapping
			var sessionId = Convert.ToBase64String(IdentityModel.CryptoRandom.CreateRandomKey(32));
			var requests = CloudToOnpremiseRequestController.requests;
			{
				requests.TryAdd(sessionId, new CloudToOnpremiseRequestController.Request
				{
					Id = sessionId,
					TenantId = tenantId,
					AbsoluteUri = request.RequestUri.AbsoluteUri,
					Content = await request.Content.ReadAsStringAsync()
				});
			}

			for (int i = 0; i < 5; i++)
			{
				await Task.Delay(TimeSpan.FromSeconds(3));

				var delegatedRequest = requests.Select(r => r.Value).FirstOrDefault(r => r.Id == sessionId);
				if (delegatedRequest != null)
				{
					if (delegatedRequest.ResponseStatusCode != null)
					{
						var response = new HttpResponseMessage((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), delegatedRequest.ResponseStatusCode));
						response.Content = new StringContent(delegatedRequest.ResponseContent, Encoding.UTF8, "application/json");
						requests.TryRemove(delegatedRequest.Id, out var value);
						logger.LogInformation("Successfully requested {CloudRequestId} from on-premise: {Result}", delegatedRequest.Id, delegatedRequest.ResponseStatusCode);
						return response;
					}
					else
					{
						continue;
					}
				}
				else
				{
					continue;
				}
			}
			{
				var response = new HttpResponseMessage(HttpStatusCode.RequestTimeout);
				response.Content = new StringContent(JsonConvert.SerializeObject(new { error = "on-premise system didn't respond within expected time" }), Encoding.UTF8, "application/json");
				requests.TryRemove(sessionId, out var value);
				logger.LogInformation("Timeout requesting {CloudRequestId} from on-premise", sessionId);
				return response;
			}
		}
	}




	public class RequestFromOnPremiseHttpMessageHandler : FuncableHttpMessageHandler
	{
		public RequestFromOnPremiseHttpMessageHandler(RequestFromOnPremise requestFromOnPremise, string name)
		{
			Sender = async req =>
			{
				var res = await requestFromOnPremise.Do(name, req);
				return res;
			};
		}
	}
}