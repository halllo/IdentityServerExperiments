using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityServer.ExternalAuth
{
    /// <summary>
    /// Should be instantiated from tenant container.
    /// </summary>
    public class ExternalLoginSchemeUpdater
    {
        public static IReadOnlyCollection<ExternalLoginScheme> GetDesiredExternalLoginSchemes()
        {
            List<ExternalLoginScheme> externalLoginSchemes = new List<ExternalLoginScheme>();

            externalLoginSchemes.Add(new ExternalLoginScheme
            {
                //?
            });

            return externalLoginSchemes.AsReadOnly();
        }

        public class ExternalLoginScheme
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Tenant { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string AuthorizationEndpoint { get; set; }
            public string TokenEndpoint { get; set; }
            public string CallbackPath { get; set; }
        }






        private readonly IAuthenticationSchemeProvider schemeProvider;
        private readonly IOptionsMonitorCache<MicrosoftAccountOptions> optionsCache;
        private readonly IEnumerable<IConfigureOptions<MicrosoftAccountOptions>> optionsConfigure;
        private readonly IEnumerable<IPostConfigureOptions<MicrosoftAccountOptions>> optionsPostConfigure;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IResolvedTenant resolvedTenant;
        private readonly ILogger<ExternalLoginSchemeUpdater> logger;

        public ExternalLoginSchemeUpdater(
            IAuthenticationSchemeProvider schemeProvider,
            IOptionsMonitorCache<MicrosoftAccountOptions> optionsCache,
            IEnumerable<IConfigureOptions<MicrosoftAccountOptions>> optionsConfigure,
            IEnumerable<IPostConfigureOptions<MicrosoftAccountOptions>> optionsPostConfigure,
            IHttpContextAccessor httpContextAccessor,
            IResolvedTenant resolvedTenant,
            ILogger<ExternalLoginSchemeUpdater> logger)
        {
            this.schemeProvider = schemeProvider;
            this.optionsCache = optionsCache;
            this.optionsConfigure = optionsConfigure;
            this.optionsPostConfigure = optionsPostConfigure;
            this.httpContextAccessor = httpContextAccessor;
            this.resolvedTenant = resolvedTenant;
            this.logger = logger;
        }

        public async Task Update(IEnumerable<ExternalLoginScheme> desiredExternalSchemes)
        {
            var desiredExternalSchemesArray = desiredExternalSchemes.ToArray();
            var actualSchemes = schemeProvider.GetAllSchemesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var actualExternalSchemes = actualSchemes.Where(scheme => scheme.Name.StartsWith("Scheme")).ToArray();
            var schemesToDelete = actualExternalSchemes.Where(scheme => !desiredExternalSchemes.Any(s => s.Name == scheme.Name)).ToArray();
            var schemesToCreate = desiredExternalSchemes.Where(scheme => !actualExternalSchemes.Any(s => s.Name == scheme.Name)).ToArray();
            var schemesToUpdate = desiredExternalSchemes.Join(actualExternalSchemes, s => s.Name, s => s.Name, (desiredExternalScheme, actualExternalScheme) => new { desiredExternalScheme, actualExternalScheme }).ToArray();

            foreach (var schemeToDelete in schemesToDelete)
            {
                RemoveScheme(schemeToDelete);
            }
            foreach (var schemeToCreate in schemesToCreate)
            {
                await AddScheme(schemeToCreate);
            }
            foreach (var schemeToUpdate in schemesToUpdate)
            {
                await UpdateScheme(schemeToUpdate.desiredExternalScheme, schemeToUpdate.actualExternalScheme);
            }
        }

        private void RemoveScheme(AuthenticationScheme scheme)
        {
            schemeProvider.RemoveScheme(scheme.Name);
            optionsCache.TryRemove(scheme.Name);

            logger.LogInformation("Removed external authentication {ExternalLogin} for tenant {Tenant}.", scheme.Name, resolvedTenant.TenantName);
        }

        private async Task AddScheme(ExternalLoginScheme scheme)
        {
            if (!new Regex("^Scheme[0-9]$").IsMatch(scheme.Name ?? string.Empty)) throw new ArgumentException($"Scheme name '{scheme.Name}' must be like '^Scheme[0-9]$'.");
            if (await schemeProvider.GetSchemeAsync(scheme.Name) != null) throw new ArgumentException($"Scheme '{scheme.Name}' already exists.");
            if (!new Regex("^[ a-zA-Z0-9]+$").IsMatch(scheme.DisplayName ?? string.Empty)) throw new ArgumentException($"Scheme display name '{scheme.DisplayName}' must be like '^[ a-zA-Z0-9]+$'.");

            var newOptions = new MicrosoftAccountOptions
            {
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                ClientId = scheme.ClientId,
                ClientSecret = scheme.ClientSecret,
                AuthorizationEndpoint = scheme.AuthorizationEndpoint,
                TokenEndpoint = scheme.TokenEndpoint,
                CallbackPath = scheme.CallbackPath,
            };
            newOptions.Validate();
            foreach (var c in optionsConfigure) c.Configure(newOptions);
            foreach (var c in optionsPostConfigure) c.PostConfigure(scheme.Name, newOptions);

            var dataProtector = newOptions.DataProtectionProvider.CreateProtector(typeof(CacheStateLocallyAndOnlySendReference).FullName, scheme.Name, "v1");
            newOptions.StateDataFormat = new CacheStateLocallyAndOnlySendReference(this.httpContextAccessor, dataProtector);

            optionsCache.TryAdd(scheme.Name, newOptions);
            schemeProvider.AddScheme(new AuthenticationScheme(scheme.Name, scheme.DisplayName, typeof(MicrosoftAccountHandler)));

            logger.LogInformation("Added external authentication {ExternalLogin} for tenant {Tenant}.", scheme.Name, resolvedTenant.TenantName);
        }

        private async Task UpdateScheme(ExternalLoginScheme desiredExternalScheme, AuthenticationScheme actualExternalScheme)
        {
            var options = optionsCache.GetOrAdd(desiredExternalScheme.Name, () => new MicrosoftAccountOptions());
            bool updateNeeded =
                desiredExternalScheme.DisplayName != actualExternalScheme.DisplayName
                ||
                desiredExternalScheme.ClientId != options.ClientId
                ||
                desiredExternalScheme.ClientSecret != options.ClientSecret
                ||
                desiredExternalScheme.AuthorizationEndpoint != options.AuthorizationEndpoint
                ||
                desiredExternalScheme.TokenEndpoint != options.TokenEndpoint
                ||
                desiredExternalScheme.CallbackPath != options.CallbackPath
                ;

            if (updateNeeded)
            {
                RemoveScheme(actualExternalScheme);
                await AddScheme(desiredExternalScheme);
            }
        }
    }
}
