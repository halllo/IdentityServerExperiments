using System;
using Microsoft.Extensions.Logging;

namespace IdentityServer
{
	public class DisposeTest : IDisposable
	{
		private readonly ILogger<DisposeTest> logger;

		public DisposeTest(ILogger<DisposeTest> logger)
		{
			this.logger = logger;
			this.logger.LogInformation("ctor");
		}

		public void Dispose() => this.logger.LogInformation("dispose");
	}
}
