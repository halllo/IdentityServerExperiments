using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api
{
	public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
		{
			// If user does not have the scope claim, get out of here
			if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
				return Task.CompletedTask;

			// Split the scopes string into an array
			var scopes = context.User.Claims.Where(c => c.Type == "scope" && c.Issuer == requirement.Issuer).Select(c => c.Value).ToArray();

			// Succeed if the scope array contains the required scope
			if (scopes.Any(s => s == requirement.Scope))
				context.Succeed(requirement);

			return Task.CompletedTask;
		}
	}

	public class HasScopeRequirement : IAuthorizationRequirement
	{
		public string Issuer { get; }
		public string Scope { get; }

		public HasScopeRequirement(string scope, string issuer)
		{
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
			Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
		}
	}
}
