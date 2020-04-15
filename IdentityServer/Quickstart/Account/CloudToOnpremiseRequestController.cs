using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Quickstart.Account
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize] with client credentials
	public class CloudToOnpremiseRequestController : ControllerBase
	{
		//store this in db
		internal static ConcurrentDictionary<string, Request> requests = new ConcurrentDictionary<string, Request>();

		internal class Request
		{
			public string Id { get; set; }
			public string TenantId { get; set; }

			public string AbsoluteUri { get; set; }
			public string Content { get; set; }

			public string ResponseStatusCode { get; set; }
			public string ResponseContent { get; set; }
		}





		public string TenantId => "localhost" //"manuels-identityserver"//User.TenantId()
			;


		[HttpGet]
		public ActionResult GetRequests()
		{
			var tenantId = TenantId;
			var rs = requests.Select(r => r.Value).Where(r => r.TenantId == tenantId && r.ResponseStatusCode == null).Select(r => NewRequestDto(r)).ToList();
			return Ok(rs);
		}

		[HttpPost]
		public ActionResult Respond([FromBody]ResponseDto response)
		{
			var tenantId = TenantId;
			var rs = requests.Select(r => r.Value).Where(r => r.TenantId == tenantId && r.ResponseStatusCode == null && r.Id == response.Id).SingleOrDefault();
			if (rs != null)
			{
				rs.ResponseStatusCode = response.StatusCode;
				rs.ResponseContent = response.Content;
				return Accepted();
			}
			else
			{
				return StatusCode((int)HttpStatusCode.Ambiguous);
			}
		}

		public class RequestDto
		{
			public string Id { get; set; }

			public string AbsoluteUri { get; set; }
			public string Content { get; set; }
		}
		private RequestDto NewRequestDto(Request r) => new RequestDto
		{
			Id = r.Id,
			AbsoluteUri = r.AbsoluteUri,
			Content = r.Content,
		};

		public class ResponseDto
		{
			public string Id { get; set; }

			public string StatusCode { get; set; }
			public string Content { get; set; }
		}
	}
}