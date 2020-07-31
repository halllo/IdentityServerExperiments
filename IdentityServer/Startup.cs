using System.Collections.Generic;
using Autofac;
using IdentityModel;
using IdentityServer.Extended;
using IdentityServer.ExternalAuth;
using IdentityServer4;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MultiTenancy;
using Newtonsoft.Json;

namespace IdentityServer
{
    public class Startup
    {
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration config;

        public Startup(IWebHostEnvironment env, IConfiguration config)
        {
            this.env = env;
            this.config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMultiTenancy();

            services.AddCors(options => options.AddPolicy("mycustomcorspolicy", b => b.WithOrigins("http://meinetollewebsite.de").AllowAnyMethod().AllowAnyHeader()));
            services.AddMvc();

            var serviceProvider = services.BuildServiceProvider();//oh oh...

            services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieOptions>();
            services.AddIdentityServer()
                .AddSigningCredentialFromKeyVault(config, serviceProvider.GetService<ILogger<Startup>>())
                .AddInMemoryIdentityResources(IdentityConfig.GetIdentityResources())
                .AddInMemoryApiResources(IdentityConfig.GetApis())
                .AddInMemoryApiScopes(IdentityConfig.GetScopes())
                .AddInMemoryClients(IdentityConfig.GetClients())
                .AddTestUsers(IdentityConfig.GetTestUsers())
                ;

            services.AddScoped<DisposeTest>();
            services.AddAuthentication();
            services.AddAuthorization(o =>
            {
                o.AddPolicy("default", b =>
                {
                    b.RequireAuthenticatedUser();
                    b.RequireClaim(JwtClaimTypes.Subject);//windows authenticated but no authenticated cookie (logged out) shouldbe treated as unauthenticated.
                });
            });

            services.AddTransient<RequestFromOnPremise>();

            services.AddTransientDecorator<ICorsPolicyProvider, CorsPolicyProvider>();
            services.AddTransientDecorator<IAuthorizeRequestValidator, ExtendedAuthorizeRequestValidator>();

            services.AddSingleton<IResolvedTenant>(new ResolvedTenant(""));
        }

        public static void ConfigureMultiTenantServices(string tenant, IServiceCollection services, IContainer applicationContainer)
        {
            var config = applicationContainer.Resolve<IConfiguration>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => { return true; };
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton<IResolvedTenant>(new ResolvedTenant(tenant));

            services.AddAuthenticationMinimumForTenant()
                .PrepareOidcScheme()
                .PrepareOAuthScheme<MicrosoftAccountOptions, MicrosoftAccountHandler>();


            if (tenant == "123")
            {
                var authenticationBuilder = new AuthenticationBuilder(services);
                authenticationBuilder.AddMicrosoftAccount("Microsoft", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = config["MicrosoftAccountClientId"];
                    options.ClientSecret = config["MicrosoftAccountClientSecret"];
                    options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{config["MicrosoftAccountTenantId"]}/oauth2/v2.0/authorize";
                    options.TokenEndpoint = $"https://login.microsoftonline.com/{config["MicrosoftAccountTenantId"]}/oauth2/v2.0/token";
                });
            }
            else if (tenant == "win")
            {
                services.Configure<AuthenticationOptions>(o =>
                {
                    o.AddScheme(IISServerDefaults.AuthenticationScheme, scheme =>
                    {
                        scheme.HandlerType = typeof(IISServerAuthenticationHandler);
                        scheme.DisplayName = IISServerDefaults.AuthenticationScheme + "!";
                    });
                });
            }
            else
            {
                var requestFromOnPremise = applicationContainer.Resolve<RequestFromOnPremise>();
                var authenticationBuilder = new AuthenticationBuilder(services);

                authenticationBuilder.AddOpenIdConnect("localidsrv", "Local IDSRV", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                    options.Authority = "https://localhost:44390/"; //BackchannelHttpHandler will tunnel the request to on premise agent
                    options.ClientId = "idsrv_login";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.SaveTokens = true;

                    options.BackchannelHttpHandler = new RequestFromOnPremiseHttpMessageHandler(requestFromOnPremise, tenant);
                    options.Configuration = JsonConvert.DeserializeObject<OpenIdConnectConfiguration>("{\"issuer\":\"https://localhost:44390\",\"jwks_uri\":\"https://localhost:44390/.well-known/openid-configuration/jwks\",\"authorization_endpoint\":\"https://localhost:44390/connect/authorize\",\"token_endpoint\":\"https://localhost:44390/connect/token\",\"userinfo_endpoint\":\"https://localhost:44390/connect/userinfo\",\"end_session_endpoint\":\"https://localhost:44390/connect/endsession\",\"check_session_iframe\":\"https://localhost:44390/connect/checksession\",\"revocation_endpoint\":\"https://localhost:44390/connect/revocation\",\"introspection_endpoint\":\"https://localhost:44390/connect/introspect\",\"device_authorization_endpoint\":\"https://localhost:44390/connect/deviceauthorization\",\"frontchannel_logout_supported\":true,\"frontchannel_logout_session_supported\":true,\"backchannel_logout_supported\":true,\"backchannel_logout_session_supported\":true,\"scopes_supported\":[\"openid\",\"profile\",\"book.read\",\"book.write\",\"offline_access\"],\"claims_supported\":[\"sub\",\"name\",\"family_name\",\"given_name\",\"middle_name\",\"nickname\",\"preferred_username\",\"profile\",\"picture\",\"website\",\"gender\",\"birthdate\",\"zoneinfo\",\"locale\",\"updated_at\"],\"grant_types_supported\":[\"authorization_code\",\"client_credentials\",\"refresh_token\",\"implicit\",\"password\",\"urn:ietf:params:oauth:grant-type:device_code\"],\"response_types_supported\":[\"code\",\"token\",\"id_token\",\"id_token token\",\"code id_token\",\"code token\",\"code id_token token\"],\"response_modes_supported\":[\"form_post\",\"query\",\"fragment\"],\"token_endpoint_auth_methods_supported\":[\"client_secret_basic\",\"client_secret_post\"],\"id_token_signing_alg_values_supported\":[\"RS256\"],\"subject_types_supported\":[\"public\"],\"code_challenge_methods_supported\":[\"plain\",\"S256\"],\"request_parameter_supported\":true}");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = JsonConvert.DeserializeObject<Jwks>("{\"keys\":[{\"kty\":\"RSA\",\"use\":\"sig\",\"kid\":\"3ZnDeMHzya1AWdC9E-oalw\",\"e\":\"AQAB\",\"n\":\"m1Qei7u2ndJdyQ4n_uLqLRTw1Suze-VJJLHoD4roENdSAkRuFa1eh9R7nGvGKPCAKYISICu0hm_ZXTAWibQeKR4X8fcHyjfqipOL-UOp5_yUO7CyFbQ3P_5Up4dP26ZbSKTr7ak3hTGw9ZcFEd2HUY2zdoUlJw5LTAUNFGVx6EYWcIoeGwxxFmUljIJ1bVKeizHJc_rKULTC09Rzo3Gm1RXs-z7sH_6yCiXB6uBdxRUVwKHUAMTYOTi07t1zDACauIfxiT6fjfameONCjteDBbHj1DxcA-6rpvza4ahhbmRb5SgLTtPru1ax47qJccHyxiK7icMXkpKj2Zae13hHlQ\",\"alg\":\"RS256\"}]}").Keys
                    };
                });
            }
        }
        private class Jwks { public List<JsonWebKey> Keys { get; set; } }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles(); // Install IdentityServer UI: iex ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/IdentityServer/IdentityServer4.Quickstart.UI/release/get.ps1'))
            app.UseMiddleware<ScopedCookiePolicyMiddleware>();
            app.UseRouting();
            app.UseIdentityServer(new IdentityServerMiddlewareOptions { AuthenticationMiddleware = app => app.UseMiddleware<ScopedAuthenticationMiddleware>() });
            app.UseCors("mycustomcorspolicy");//always after UseIdentityServer
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }





    internal class ConfigureCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        public ConfigureCookieOptions() { }
        public void Configure(CookieAuthenticationOptions options) { }
        public void Configure(string name, CookieAuthenticationOptions options)
        {
            options.AccessDeniedPath = "/Account/Login"; //access denied (http:forbidden) should be handled like unauthenticated (http:unauthorized)
        }
    }
}
