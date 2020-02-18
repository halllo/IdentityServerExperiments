// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiTenancy;

namespace IdentityServer4.Quickstart.UI
{
	[SecurityHeaders]
	[AllowAnonymous]
	public class HomeController : Controller
	{
		private readonly IIdentityServerInteractionService _interaction;
		private readonly IWebHostEnvironment _environment;
		private readonly ILogger _logger;
		private readonly IConfiguration _config;
		private readonly TemporaryTenantGuidService temporaryTenantGuidService;

		public HomeController(IIdentityServerInteractionService interaction, IWebHostEnvironment environment, ILogger<HomeController> logger, IConfiguration config, TemporaryTenantGuidService temporaryTenantGuidService)
		{
			_interaction = interaction;
			_environment = environment;
			_logger = logger;
			_config = config;
			this.temporaryTenantGuidService = temporaryTenantGuidService;
		}

		public async Task<IActionResult> Index()
		{
			return View();
		}

		public async Task<IActionResult> TemporaryTenantGuid()
		{
			return Ok(new
			{
				opid = this.temporaryTenantGuidService.Id
			});
		}

		/// <summary>
		/// Shows the error page
		/// </summary>
		public async Task<IActionResult> Error(string errorId)
		{
			var vm = new ErrorViewModel();

			// retrieve error details from identityserver
			var message = await _interaction.GetErrorContextAsync(errorId);
			if (message != null)
			{
				vm.Error = message;

				if (!_environment.IsDevelopment())
				{
					// only show in development
					message.ErrorDescription = null;
				}
			}

			return View("Error", vm);
		}
	}
}