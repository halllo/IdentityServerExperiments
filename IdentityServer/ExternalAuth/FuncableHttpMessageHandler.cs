using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityServer.ExternalAuth
{
	public class FuncableHttpMessageHandler : HttpMessageHandler
	{
		public Func<HttpRequestMessage, Task<HttpResponseMessage>> Sender { get; set; }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			if (Sender != null)
			{
				return await Sender(request);
			}

			return null;
		}
	}
}