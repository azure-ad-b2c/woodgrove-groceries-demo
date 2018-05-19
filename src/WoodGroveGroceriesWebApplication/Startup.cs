using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using WoodGroveGroceriesWebApplication.Entities;
using WoodGroveGroceriesWebApplication.EntityFramework;
using WoodGroveGroceriesWebApplication.IdentityModel.Protocols;
using WoodGroveGroceriesWebApplication.Managers;
using WoodGroveGroceriesWebApplication.Repositories;
using WoodGroveGroceriesWebApplication.ViewServices;

namespace WoodGroveGroceriesWebApplication
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
            ConfigureAuthorization(services);
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
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            ConfigureCookieAuthentication(authenticationBuilder);
            ConfigureB2CAuthentication(configuration, services, authenticationBuilder);
            ConfigureB2BAuthentication(configuration, services, authenticationBuilder);
        }

        private static void ConfigureAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.AuthorizationPolicies.AccessCatalog, policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerManager, Constants.Roles.Partner, Constants.Roles.Employee, Constants.Roles.IndividualCustomer));
                options.AddPolicy(Constants.AuthorizationPolicies.AddToCatalog, policyBuilder => policyBuilder.RequireRole(Constants.Roles.Partner, Constants.Roles.Employee));
                options.AddPolicy(Constants.AuthorizationPolicies.RemoveFromCatalog, policyBuilder => policyBuilder.RequireRole(Constants.Roles.Partner, Constants.Roles.Employee));
                options.AddPolicy(Constants.AuthorizationPolicies.AccessPantry, policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerManager, Constants.Roles.BusinessCustomerStocker));
                options.AddPolicy(Constants.AuthorizationPolicies.AddToPantry, policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerManager));
                options.AddPolicy(Constants.AuthorizationPolicies.RemoveFromPantry, policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerManager));
                options.AddPolicy(Constants.AuthorizationPolicies.AccessTrolley, policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerStocker, Constants.Roles.IndividualCustomer));
            });
        }

        private static void ConfigureB2BAuthentication(IConfiguration configuration, IServiceCollection services, AuthenticationBuilder authenticationBuilder)
        {
            var authenticationOptions = configuration.GetSection("ActiveDirectoryB2BAuthentication")
                .Get<ActiveDirectoryAuthenticationOptions>();

            authenticationBuilder.AddOpenIdConnect(Constants.AuthenticationSchemes.B2BOpenIdConnect, options =>
            {
                options.Authority = authenticationOptions.Authority;
                options.CallbackPath = new PathString("/b2b-signin-callback");
                options.ClientId = authenticationOptions.ClientId;
                options.Events = CreateB2BOpenIdConnectEvents();
                options.SignedOutCallbackPath = new PathString("/b2b-signout-callback");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = Constants.ClaimTypes.Name
                };
            });
        }

        private static void ConfigureB2CAuthentication(IConfiguration configuration, IServiceCollection services, AuthenticationBuilder authenticationBuilder)
        {
            var authenticationOptions = configuration.GetSection("ActiveDirectoryB2CAuthentication")
                .Get<ActiveDirectoryAuthenticationOptions>();

            authenticationBuilder.AddOpenIdConnect(Constants.AuthenticationSchemes.B2COpenIdConnect, options =>
            {
                options.Authority = authenticationOptions.Authority;
                options.CallbackPath = new PathString("/b2c-signin-callback");
                options.ClientId = authenticationOptions.ClientId;

                options.ConfigurationManager = new PolicyConfigurationManager(
                    authenticationOptions.Authority,
                    new[]
                    {
                        Constants.Policies.PasswordReset,
                        Constants.Policies.ProfileUpdateWithPersonalAccount,
                        Constants.Policies.ProfileUpdateWithWorkAccount,
                        Constants.Policies.SignUpOrSignInWithPersonalAccount,
                        Constants.Policies.SignUpOrSignInWithWorkAccount
                    });

                options.Events = CreateB2COpenIdConnectEvents();
                options.Scope.Remove("profile");
                options.SignedOutCallbackPath = new PathString("/b2c-signout-callback");

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
            services.AddTransient<ICatalogItemManager, CatalogItemManager>();
            services.AddTransient<IPantryManager, PantryManager>();
            services.AddTransient<ITrolleyManager, TrolleyManager>();
        }

        private static void ConfigureMvc(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                var requireHttpsFilter = new RequireHttpsAttribute();
                options.Filters.Add(requireHttpsFilter);
            });
        }

        private static void ConfigureOptions(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<DbContextInitializationOptions>(configuration.GetSection("DbContextInitialization"));
        }

        private static void ConfigureRepositories(IServiceCollection services)
        {
            services.AddScoped<ICatalogItemRepository, CatalogItemDbRepository>();
            services.AddScoped<IRepository<CatalogItem>, DbRepository<CatalogItem>>();
            services.AddScoped<IRepository<Pantry>, DbRepository<Pantry>>();
            services.AddScoped<IRepository<Trolley>, DbRepository<Trolley>>();
        }

        private static void ConfigureStores(IServiceCollection services)
        {
            services.AddDbContext<WoodGroveGroceriesDbContext>(options =>
            {
                options.UseInMemoryDatabase("WoodGroveGroceries");
            }, ServiceLifetime.Singleton);
        }

        private static void ConfigureViewServices(IServiceCollection services)
        {
            services.AddTransient<ICatalogItemViewService, CatalogItemViewService>();
            services.AddTransient<IPantryViewService, PantryViewService>();
            services.AddTransient<ITrolleyViewService, TrolleyViewService>();
        }

        private static OpenIdConnectEvents CreateB2BOpenIdConnectEvents()
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
                    string ownerIdClaimValue;
                    string roleClaimValue;
                    var identityProvider = context.Principal.FindFirstValue(Constants.ClaimTypes.IdentityProvider);

                    if (!string.IsNullOrEmpty(identityProvider))
                    {
                        ownerIdClaimValue = identityProvider.StartsWith("https://sts.windows.net/") ? identityProvider : $"{identityProvider}/{context.Principal.FindFirstValue(Constants.ClaimTypes.ObjectIdentifier)}";
                        roleClaimValue = Constants.Roles.Partner;
                    }
                    else
                    {
                        ownerIdClaimValue = context.Principal.FindFirstValue(Constants.ClaimTypes.Issuer);
                        roleClaimValue = Constants.Roles.Employee;
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(Constants.ClaimTypes.OwnerIdentifier, ownerIdClaimValue),
                        new Claim(ClaimTypes.Role, roleClaimValue)
                    };

                    var identity = new ClaimsIdentity(claims);
                    context.Principal.AddIdentity(identity);
                    return Task.CompletedTask;
                }
            };
        }

        private static OpenIdConnectEvents CreateB2COpenIdConnectEvents()
        {
            return new OpenIdConnectEvents
            {
                OnAuthenticationFailed = context =>
                {
                    context.Fail(context.Exception);
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.Error) && !string.IsNullOrEmpty(context.ProtocolMessage.ErrorDescription))
                    {
                        if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C90091"))
                        {
                            var policy = context.Properties.Items[Constants.AuthenticationProperties.Policy];

                            if (policy == Constants.Policies.PasswordReset || policy == Constants.Policies.SignUpOrSignInWithPersonalAccount)
                            {
                                var command = $"{Constants.AuthenticationSchemes.B2COpenIdConnect}:{Constants.Policies.SignUpOrSignInWithPersonalAccount}";
                                var uiLocale = context.Properties.Items[Constants.AuthenticationProperties.UILocales];
                                context.Response.Redirect($"/Account/LogInFor?command={command}&uiLocale={uiLocale}");
                                context.HandleResponse();
                            }
                            else
                            {
                                context.Response.Redirect("/");
                                context.HandleResponse();
                            }
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C90118"))
                        {
                            var uiLocale = context.Properties.Items[Constants.AuthenticationProperties.UILocales];
                            context.Response.Redirect($"/Account/ResetPassword?uiLocale={uiLocale}");
                            context.HandleResponse();
                        }
                    }

                    return Task.CompletedTask;
                },
                OnRedirectToIdentityProvider = async context =>
                {
                    var policy = context.Properties.Items.ContainsKey(Constants.AuthenticationProperties.Policy) ? context.Properties.Items[Constants.AuthenticationProperties.Policy] : Constants.Policies.SignUpOrSignInWithPersonalAccount;
                    var configuration = await GetB2COpenIdConnectConfigurationAsync(context, policy);
                    context.ProtocolMessage.IssuerAddress = configuration.AuthorizationEndpoint;

                    if (context.Properties.Items.ContainsKey(Constants.AuthenticationProperties.UILocales))
                    {
                        context.ProtocolMessage.SetParameter("ui_locales", context.Properties.Items[Constants.AuthenticationProperties.UILocales]);
                    }

                    context.ProtocolMessage.SetParameter("dc", "cdm");
                    context.ProtocolMessage.SetParameter("slice", "001-000");
                },
                OnRedirectToIdentityProviderForSignOut = async context =>
                {
                    var configuration = await GetB2COpenIdConnectConfigurationAsync(context, context.Properties.Items[Constants.AuthenticationProperties.Policy]);
                    context.ProtocolMessage.IssuerAddress = configuration.EndSessionEndpoint;
                },
                OnTokenValidated = context =>
                {
                    var requestedPolicy = context.Properties.Items[Constants.AuthenticationProperties.Policy];
                    var issuedPolicy = context.Principal.FindFirstValue(Constants.ClaimTypes.TrustFrameworkPolicy);

                    if (!string.Equals(issuedPolicy, requestedPolicy, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Fail($"Access denied: The issued policy '{issuedPolicy}' is different to the requested policy '{requestedPolicy}'.");
                        return Task.CompletedTask;
                    }

                    string roleClaimValue;
                    var identityProvider = context.Principal.FindFirstValue(Constants.ClaimTypes.IdentityProvider);

                    if (identityProvider != null && identityProvider.StartsWith("https://sts.windows.net/"))
                    {
                        var businessCustomerRole = context.Principal.FindFirstValue(Constants.ClaimTypes.BusinessCustomerRole);

                        if (businessCustomerRole == "Manager")
                        {
                            roleClaimValue = Constants.Roles.BusinessCustomerManager;
                        }
                        else
                        {
                            roleClaimValue = Constants.Roles.BusinessCustomerStocker;
                        }
                    }
                    else
                    {
                        roleClaimValue = Constants.Roles.IndividualCustomer;
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Role, roleClaimValue)
                    };

                    var identity = new ClaimsIdentity(claims);
                    context.Principal.AddIdentity(identity);
                    return Task.CompletedTask;
                }
            };
        }

        private static Task<OpenIdConnectConfiguration> GetB2COpenIdConnectConfigurationAsync(RedirectContext context, string policy)
        {
            var configurationManager = (PolicyConfigurationManager)context.Options.ConfigurationManager;
            return configurationManager.GetConfigurationForPolicyAsync(policy, CancellationToken.None);
        }
    }
}
