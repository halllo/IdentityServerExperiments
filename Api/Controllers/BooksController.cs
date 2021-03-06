﻿using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
	[Route("api/books")]
	public class BooksController : Controller
	{
		[HttpGet]
		[Authorize("book.read")]
		public IActionResult Get()
		{
			return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
		}

		[HttpPost]
		[Authorize("book.write")]
		public IActionResult Post()
		{
			return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
		}



		[HttpGet]
		[Route("help")]
		[AllowAnonymous]
		public IActionResult GetHelp()
		{
			return Ok("ok");
		}
	}
}
