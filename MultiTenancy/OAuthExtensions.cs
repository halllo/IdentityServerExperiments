using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MultiTenancy
{
	/// <summary>
	/// Combination of <see cref="OAuthExtensions"/> 
	/// and <see cref="AuthenticationBuilder"/> for use in multi tenant contexts.
	/// </summary>
	public static class OAuthExtensions
	{
		/// <summary>
		/// Has to be invoked on the application container.
		/// </summary>
		/// <typeparam name="THandler"></typeparam>
		/// <param name="services"></param>
		/// <param name="authenticationScheme"></param>
		/// <param name="displayName"></param>
		public static void PrepareMultiTenantOAuth<THandler>(this IServiceCollection services, string authenticationScheme, string displayName)
		{
			services.Configure<AuthenticationOptions>(o =>
			{
				o.AddScheme(authenticationScheme, scheme =>
				{
					scheme.HandlerType = typeof(THandler);
					scheme.DisplayName = displayName;
				});
			});
		}

		/// <summary>
		/// Has to be invoked on the tenant container.
		/// </summary>
		/// <typeparam name="TOptions"></typeparam>
		/// <typeparam name="THandler"></typeparam>
		/// <param name="services"></param>
		/// <param name="authenticationScheme"></param>
		/// <param name="configureOptions"></param>
		public static void AddMultiTenantOAuth<TOptions, THandler>(this IServiceCollection services, string authenticationScheme, Action<TOptions> configureOptions)
		  where TOptions : OAuthOptions, new()
		  where THandler : OAuthHandler<TOptions>
		{
			services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<TOptions>, OAuthPostConfigureOptions<TOptions, THandler>>());
			services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<TOptions>, EnsureSignInScheme<TOptions>>());

			if (configureOptions != null)
			{
				services.Configure(authenticationScheme, configureOptions);
			}
			services.AddOptions<TOptions>(authenticationScheme).Validate(o =>
			{
				o.Validate(authenticationScheme);
				return true;
			});
			services.AddTransient<THandler>();
		}

		// Used to ensure that there's always a default sign in scheme that's not itself
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
