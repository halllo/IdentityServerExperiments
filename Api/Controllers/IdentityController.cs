using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
	[Route("api/id1")]
	[Authorize("Id1")]
	public class IdentityController : Controller
	{
		[HttpGet]
		public IActionResult Get()
		{
			return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
		}
	}


	[Route("api/id2")]
	[Authorize("Id2")]
	public class Identity2Controller : Controller
	{
		[HttpGet]
		public IActionResult Get()
		{
			return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
		}
	}
}
