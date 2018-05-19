using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WoodGroveAdministrationWebApplication.IdentityModel.Clients.ActiveDirectory;
using WoodGroveAdministrationWebApplication.Managers;
using WoodGroveAdministrationWebApplication.Repositories;
using WoodGroveAdministrationWebApplication.ViewServices;

namespace WoodGroveAdministrationWebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            var rewriteOptions = new RewriteOptions()
                .AddRedirectToHttps();

            app.UseRewriter(rewriteOptions);
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureOptions(Configuration, services);
            ConfigureAuthentication(Configuration, services);
            ConfigureMvc(services);
            ConfigureViewServices(services);
            ConfigureManagers(services);
            ConfigureRepositories(services);
            ConfigureStores(services);
        }

        private static void ConfigureAuthentication(IConfiguration configuration, IServiceCollection services)
        {
            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            ConfigureCookieAuthentication(authenticationBuilder);
            ConfigureB2EAuthentication(configuration, services, authenticationBuilder);
        }

        private static void ConfigureB2EAuthentication(IConfiguration configuration, IServiceCollection services, AuthenticationBuilder authenticationBuilder)
        {
            var authenticationSettings = configuration.GetSection("ActiveDirectoryAuthentication")
                .Get<ActiveDirectoryAuthenticationOptions>();

            authenticationBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = authenticationSettings.Authority;
                options.CallbackPath = new PathString("/signin-callback");
                options.ClientId = authenticationSettings.ClientId;
                options.Events = CreateB2EOpenIdConnectEvents();
                options.SignedOutCallbackPath = new PathString("/signout-callback");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = Constants.ClaimTypes.Name
                };
            });
        }

        private static void ConfigureCookieAuthentication(AuthenticationBuilder authenticationBuilder)
        {
            authenticationBuilder.AddCookie();
        }

        private static void ConfigureManagers(IServiceCollection services)
        {
            services.AddTransient<IUserManager, UserManager>();
        }

        private static void ConfigureMvc(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                var requireHttpsFilter = new RequireHttpsAttribute();
                options.Filters.Add(requireHttpsFilter);

                var authorizationPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                var authorizeFilter = new AuthorizeFilter(authorizationPolicy);
                options.Filters.Add(authorizeFilter);
            });
        }

        private static void ConfigureOptions(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<ActiveDirectoryGraphOptions>(configuration.GetSection("ActiveDirectoryGraph"));
            services.Configure<PowerBIReportOptions>(configuration.GetSection("PowerBIReport"));
        }

        private static void ConfigureRepositories(IServiceCollection services)
        {
            services.AddTransient<IUserRepository, GraphUserRepository>();
        }

        private static void ConfigureStores(IServiceCollection services)
        {
            services.AddSingleton<IAuthenticationContext, ActiveDirectoryAuthenticationContext>();
            services.AddSingleton<HttpClient>();
        }

        private static void ConfigureViewServices(IServiceCollection services)
        {
            services.AddTransient<IUserViewService, UserViewService>();
        }

        private static OpenIdConnectEvents CreateB2EOpenIdConnectEvents()
        {
            return new OpenIdConnectEvents
            {
                OnAuthenticationFailed = context =>
                {
                    context.Fail(context.Exception);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var identityProvider = context.Principal.FindFirstValue(Constants.ClaimTypes.IdentityProvider);

                    if (!string.IsNullOrEmpty(identityProvider))
                    {
                        context.Fail("Access denied.");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }

                    return Task.CompletedTask;
                }
            };
        }
    }
}
