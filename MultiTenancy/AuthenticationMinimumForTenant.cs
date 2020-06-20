using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MultiTenancy
{
    public static class AuthenticationMinimumForTenant
    {
        public static Builder AddAuthenticationMinimumForTenant(this ServiceCollection services)
        {
            /*
			 * Don't invoke services.AddAuthentication(); in each tenant service configuration!
			 * Instead reuse IAuthenticationService and IClaimsTransformation registrations for all tenant containers.
			 * Having a new IAuthenticationService seems to break IS4's idsrv.session cookies.
			 */
            services.TryAddScoped<IAuthenticationHandlerProvider, AuthenticationHandlerProvider>();
            services.TryAddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();

            return new Builder(services);
        }

        public static Builder PrepareOidcScheme(this Builder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, EnsureSignInScheme<OpenIdConnectOptions>>());
            builder.Services.TryAddTransient<OpenIdConnectHandler>();
            return builder;
        }

        public static Builder PrepareOAuthScheme<TOptions, THandler>(this Builder builder)
            where TOptions : OAuthOptions, new()
            where THandler : OAuthHandler<TOptions>
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<TOptions>, OAuthPostConfigureOptions<TOptions, THandler>>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<TOptions>, EnsureSignInScheme<TOptions>>());
            builder.Services.TryAddTransient<THandler>();
            return builder;
        }


        public class Builder
        {
            public IServiceCollection Services { get; }
            public Builder(IServiceCollection services)
            {
                Services = services;
            }
        }

        private class EnsureSignInScheme<TOptions> : IPostConfigureOptions<TOptions> where TOptions : RemoteAuthenticationOptions
        {
            private readonly AuthenticationOptions _authOptions;

            public EnsureSignInScheme(IOptions<AuthenticationOptions> authOptions)
            {
                _authOptions = authOptions.Value;
            }

            public void PostConfigure(string name, TOptions options)
            {
                options.SignInScheme = options.SignInScheme ?? _authOptions.DefaultSignInScheme ?? _authOptions.DefaultScheme;
            }
        }
    }
}
