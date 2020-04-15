namespace WoodGroveGroceriesWebApplication
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Entities;
    using EntityFramework;
    using Extensions;
    using IdentityModel.Protocols;
    using Managers;
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
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Models.Settings;
    using Newtonsoft.Json;
    using Repositories;
    using Services;
    using ViewServices;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();

                IdentityModelEventSource.ShowPII = true;
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

            app.UseMvc(routes => { routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}"); });
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
            ConfigureIIS(services);
        }

        private void ConfigureAuthentication(IConfiguration configuration, IServiceCollection services)
        {
            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            var manager = new PolicyManager(configuration);
            services.AddSingleton(manager);

            ConfigureCookieAuthentication(authenticationBuilder);
            ConfigureBetaAppAuthentication(configuration, services, authenticationBuilder);
            ConfigureCustomerAuthentication(configuration, services, authenticationBuilder, manager);
            ConfigureBusinessCustomerAuthentication(configuration, authenticationBuilder, manager);
            ConfigurePartnerAuthentication(configuration, authenticationBuilder);
        }

        private void ConfigureAuthorization(IServiceCollection services)
        {
            var betaAccessOptions = AuthenticationBetaAppAccessOptions.Construct(Configuration);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.AuthorizationPolicies.AccessCatalog,
                    policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerManager, Constants.Roles.Partner,
                        Constants.Roles.Employee, Constants.Roles.IndividualCustomer));
                options.AddPolicy(Constants.AuthorizationPolicies.AddToCatalog,
                    policyBuilder => policyBuilder.RequireRole(Constants.Roles.Partner, Constants.Roles.Employee));
                options.AddPolicy(Constants.AuthorizationPolicies.RemoveFromCatalog,
                    policyBuilder => policyBuilder.RequireRole(Constants.Roles.Partner, Constants.Roles.Employee));
                options.AddPolicy(Constants.AuthorizationPolicies.AccessPantry,
                    policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerManager, Constants.Roles.BusinessCustomerStocker));
                options.AddPolicy(Constants.AuthorizationPolicies.AddToPantry,
                    policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerManager));
                options.AddPolicy(Constants.AuthorizationPolicies.RemoveFromPantry,
                    policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerManager));
                options.AddPolicy(Constants.AuthorizationPolicies.AccessTrolley,
                    policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerStocker, Constants.Roles.IndividualCustomer));
                //options.AddPolicy(Constants.AuthorizationPolicies.AccessCheckout, policyBuilder => policyBuilder.Requirements.Add(new MfaRequirement("B2C_1A_WoodGrove_Dev_mfa")));
                options.AddPolicy(Constants.AuthorizationPolicies.AccessCheckout,
                    policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerStocker, Constants.Roles.IndividualCustomer));

                options.AddPolicy(Constants.AuthorizationPolicies.ChangeUserRole,
                    policyBuilder => policyBuilder.RequireRole(Constants.Roles.BusinessCustomerStocker, Constants.Roles.BusinessCustomerManager));

                options.AddPolicy(Constants.AuthorizationPolicies.BetaAppAccess, policyBuilder =>
                {
                    if (betaAccessOptions.RequireFullAppAuth)
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.AddAuthenticationSchemes(
                            Constants.AuthenticationSchemes.CustomerAuth,
                            Constants.AuthenticationSchemes.BusinessCustomerAuth,
                            Constants.AuthenticationSchemes.PartnerOpenIdConnect,
                            Constants.AuthenticationSchemes.BetaAccessOpenIdConnect
                        );
                    }
                    else
                    {
                        policyBuilder.RequireAssertion(_ => true);
                    }
                });
            });
        }

        private void ConfigurePartnerAuthentication(IConfiguration configuration, AuthenticationBuilder authenticationBuilder)
        {
            var authenticationOptions = AuthenticationPartnerOptions.Construct(Configuration);

            authenticationBuilder.AddOpenIdConnect(Constants.AuthenticationSchemes.PartnerOpenIdConnect, options =>
            {
                options.Authority = authenticationOptions.Authority;
                options.CallbackPath = new PathString("/partner-signin-callback");
                options.ClientId = authenticationOptions.ClientId;
                options.Events = CreatePartnerOpenIdConnectEvents();
                options.SignedOutCallbackPath = new PathString("/partner-signout-callback");
                //options.Scope.Clear();
                //options.Scope.Add("openid");

                options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = Constants.ClaimTypes.Name };
            });
        }

        private void ConfigureBusinessCustomerAuthentication(
            IConfiguration configuration,
            AuthenticationBuilder authenticationBuilder,
            PolicyManager manager)
        {
            var authenticationOptions = AuthenticationCustomerOptions.Construct(Configuration);

            var policyList = manager.BusinessCustomerPolicySetupList;

            // BusinessCustomerPolicySetupList
            authenticationBuilder.AddOpenIdConnect(Constants.AuthenticationSchemes.BusinessCustomerAuth, options =>
            {
                options.Authority = authenticationOptions.Authority;
                options.CallbackPath = new PathString("/b2b-signin-callback");
                options.ClientId = authenticationOptions.ClientId;
                options.CorrelationCookie.Expiration = TimeSpan.FromHours(3);

                options.ConfigurationManager = new PolicyConfigurationManager(
                    authenticationOptions.Authority,
                    policyList);

                options.Events = CreateB2BOpenIdConnectEvents();
                options.SignedOutCallbackPath = new PathString("/b2b-signout-callback");

                options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = Constants.ClaimTypes.Name };
            });
        }

        private void ConfigureBetaAppAuthentication(IConfiguration configuration, IServiceCollection services,
            AuthenticationBuilder authenticationBuilder)
        {
            var authenticationOptions = AuthenticationBetaAppAccessOptions.Construct(configuration);

            if (authenticationOptions.RequireFullAppAuth)
            {
                authenticationBuilder.AddOpenIdConnect(Constants.AuthenticationSchemes.BetaAccessOpenIdConnect, options =>
                {
                    options.Authority = authenticationOptions.Authority;
                    options.CallbackPath = new PathString("/betaaccess-callback");
                    options.ClientId = authenticationOptions.ClientId;
                    options.ClientSecret = authenticationOptions.ClientSecret;
                    options.CorrelationCookie.Expiration = TimeSpan.FromHours(3);

                    options.ConfigurationManager = new PolicyConfigurationManager(
                        authenticationOptions.Authority,
                        new[] { authenticationOptions.Policy });

                    options.Events = CreateBetaAppOidcEvents();
                    options.Scope.Remove("profile");
                    //options.SignedOutCallbackPath = new PathString("/b2c-signout-callback");

                    options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = Constants.ClaimTypes.Name };
                });


                //authenticationBuilder.AddOpenIdConnect(Constants.AuthenticationSchemes.BetaAccessOpenIdConnect, options =>
                //{
                //    options.Authority = authenticationOptions.Authority;
                //    options.CallbackPath = new PathString("/betaaccess-callback");
                //    options.ClientId = authenticationOptions.ClientId;
                //    options.Events = CreateB2COpenIdConnectEvents();
                //    options.SignedOutCallbackPath = new PathString("/betaaccess-signout-callback");

                //    options.TokenValidationParameters = new TokenValidationParameters
                //    {
                //        NameClaimType = Constants.ClaimTypes.Name
                //    };
                //});
            }

            services.AddSingleton<IdentityService, IdentityService>();
        }

        private void ConfigureCustomerAuthentication(
            IConfiguration configuration,
            IServiceCollection services,
            AuthenticationBuilder authenticationBuilder,
            PolicyManager manager)
        {
            var authenticationOptions = AuthenticationCustomerOptions.Construct(configuration);

            var policyList = manager.CustomerPolicySetupList;

            authenticationBuilder.AddOpenIdConnect(Constants.AuthenticationSchemes.CustomerAuth, options =>
            {
                options.Authority = authenticationOptions.Authority;
                options.CallbackPath = new PathString("/b2c-signin-callback");
                options.ClientId = authenticationOptions.ClientId;
                options.ClientSecret = authenticationOptions.ClientSecret;
                options.CorrelationCookie.Expiration = TimeSpan.FromHours(3);

                options.ConfigurationManager = new PolicyConfigurationManager(
                    authenticationOptions.Authority,
                    policyList);

                options.Events = CreateB2COpenIdConnectEvents(manager);
                options.Scope.Remove("profile");
                options.SignedOutCallbackPath = new PathString("/b2c-signout-callback");

                options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = Constants.ClaimTypes.Name };
            });
        }

        private void ConfigureCookieAuthentication(AuthenticationBuilder authenticationBuilder)
        {
            authenticationBuilder.AddCookie();
        }

        private void ConfigureManagers(IServiceCollection services)
        {
            services.AddTransient<ICatalogItemManager, CatalogItemManager>();
            services.AddTransient<IPantryManager, PantryManager>();
            services.AddTransient<ITrolleyManager, TrolleyManager>();
        }

        private void ConfigureMvc(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                var requireHttpsFilter = new RequireHttpsAttribute();
                options.Filters.Add(requireHttpsFilter);
                options.EnableEndpointRouting = false;
            });
        }

        private void ConfigureOptions(IConfiguration configuration, IServiceCollection services)
        {
            services.AddScoped<IndustryManager, IndustryManager>();

            services.Configure<DbContextInitializationOptions>(configuration.GetSection("DbContextInitialization"));
        }

        private void ConfigureRepositories(IServiceCollection services)
        {
            services.AddScoped<ICatalogItemRepository, CatalogItemDbRepository>();
            services.AddScoped<IRepository<CatalogItem>, DbRepository<CatalogItem>>();
            services.AddScoped<IRepository<Pantry>, DbRepository<Pantry>>();
            services.AddScoped<IRepository<Trolley>, DbRepository<Trolley>>();
        }

        private void ConfigureStores(IServiceCollection services)
        {
            services.AddDbContext<WoodGroveGroceriesDbContext>(options => { options.UseInMemoryDatabase("WoodGroveGroceries"); },
                ServiceLifetime.Singleton);
        }

        private void ConfigureIIS(IServiceCollection services)
        {
            services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });

            services.AddSingleton<HostService, HostService>();
        }

        private void ConfigureViewServices(IServiceCollection services)
        {
            services.AddTransient<ICatalogItemViewService, CatalogItemViewService>();
            services.AddTransient<IPantryViewService, PantryViewService>();
            services.AddTransient<ITrolleyViewService, TrolleyViewService>();
        }

        private OpenIdConnectEvents CreatePartnerOpenIdConnectEvents()
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
                        ownerIdClaimValue = identityProvider.StartsWith("https://sts.windows.net/")
                            ? identityProvider
                            : $"{identityProvider}/{context.Principal.FindFirstValue(Constants.ClaimTypes.ObjectIdentifier)}";
                        roleClaimValue = Constants.Roles.Partner;
                    }
                    else
                    {
                        ownerIdClaimValue = context.Principal.FindFirstValue(Constants.ClaimTypes.Issuer);
                        roleClaimValue = Constants.Roles.Employee;
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(Constants.ClaimTypes.OwnerIdentifier, ownerIdClaimValue), new Claim(ClaimTypes.Role, roleClaimValue)
                    };

                    var identity = new ClaimsIdentity(claims);
                    context.Principal.AddIdentity(identity);
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.Error) && !string.IsNullOrEmpty(context.ProtocolMessage.ErrorDescription))
                    {
                        if (context.ProtocolMessage.ErrorDescription.StartsWith("AADSTS250004"))
                        {
                            context.Response.Redirect("/Account/LogOut");
                            context.HandleResponse();
                        }
                    }

                    return Task.CompletedTask;
                },
                OnRedirectToIdentityProvider = context =>
                {
                    context.ProtocolMessage.SetParameter("DC", "PROD-wst-TEST1");
                    context.ProtocolMessage.SetParameter("jitsignup", "true");
                    context.ProtocolMessage.SetParameter("signupux", "true");
                    context.ProtocolMessage.SetParameter("socialv2", "true");

                    return Task.CompletedTask;
                }
            };
        }

        private OpenIdConnectEvents CreateB2COpenIdConnectEvents(PolicyManager policyManager)
        {
            return new OpenIdConnectEvents
            {
                OnRemoteFailure = context =>
                {
                    if (context.Failure.Message == "Correlation failed.")
                    {
                        //// [ log error ]
                        context.HandleResponse();
                        //// redirect to some help page or handle it as you wish
                        context.Response.Redirect("/");
                    }

                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.Message.StartsWith("IDX21323"))
                    {
                        context.Response.Redirect("/Account/TimeoutError");
                        context.HandleResponse();
                    }
                    else
                    {
                        context.Fail(context.Exception);
                    }

                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    if (context.ProtocolMessage.Parameters.Any(x => x.Key == "response"))
                    {
                        var jsonResponse = JsonConvert.DeserializeObject<Constants.JsonResponse>(context.ProtocolMessage.Parameters["response"]);

                        if (jsonResponse != null && !string.IsNullOrEmpty(jsonResponse.response) && jsonResponse.response.Contains("B2C_V1_90001"))
                        {
                            context.Response.Redirect("/Account/AgeGatingError");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }
                    }

                    if (!string.IsNullOrEmpty(context.ProtocolMessage.Error) && !string.IsNullOrEmpty(context.ProtocolMessage.ErrorDescription))
                    {
                        if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C90091"))
                        {
                            var policy = context.Properties.Items[Constants.AuthenticationProperties.Policy];

                            if (policy == policyManager.PasswordReset ||
                                policy == policyManager.SignUpOrSignInWithPersonalAccountLocalEmailAndSocial)
                            {
                                var command =
                                    $"{Constants.AuthenticationSchemes.CustomerAuth}:{policyManager.SignUpOrSignInWithPersonalAccountLocalEmailAndSocial}";

                                var uiLocale = string.Empty;

                                if (context.Properties.Items.Any(x => x.Key == Constants.AuthenticationProperties.UILocales))
                                {
                                    uiLocale = context.Properties.Items[Constants.AuthenticationProperties.UILocales];
                                }

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
                            var uiLocale = string.Empty;

                            if (context.Properties.Items.Any(x => x.Key == Constants.AuthenticationProperties.UILocales))
                            {
                                uiLocale = context.Properties.Items[Constants.AuthenticationProperties.UILocales];
                            }

                            context.Response.Redirect($"/Account/ResetPassword?uiLocale={uiLocale}");
                            context.HandleResponse();
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C99001"))
                        {
                            context.Response.Redirect($"/Account/LinkError??ReturnUrl={context.Properties.RedirectUri}");
                            context.HandleResponse();
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C99002"))
                        {
                            context.Response.Redirect("/Account/LogOut");
                            context.HandleResponse();
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C90157"))
                        {
                            context.Response.Redirect("/Account/RetryExceededError");
                            context.HandleResponse();
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C90037"))
                        {
                            context.Response.Redirect("/Account/AgeGatingError");
                            context.HandleResponse();
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C90273"))
                        {
                            context.Response.Redirect("/");
                            context.HandleResponse();
                        }
                    }

                    return Task.CompletedTask;
                },
                OnRedirectToIdentityProvider = async context =>
                {
                    var policy = context.Properties.Items.ContainsKey(Constants.AuthenticationProperties.Policy)
                        ? context.Properties.Items[Constants.AuthenticationProperties.Policy]
                        : policyManager.SignUpOrSignInWithPersonalAccountLocalEmailAndSocial;
                    var configuration = await GetB2COpenIdConnectConfigurationAsync(context, policy);
                    context.ProtocolMessage.IssuerAddress = configuration.AuthorizationEndpoint;

                    if (context.Properties.Items.ContainsKey(Constants.AuthenticationProperties.UILocales))
                    {
                        context.ProtocolMessage.SetParameter("ui_locales", context.Properties.Items[Constants.AuthenticationProperties.UILocales]);
                    }

                    if (context.Properties.Items.ContainsKey(Constants.AuthenticationProperties.BgImage))
                    {
                        context.ProtocolMessage.SetParameter(Constants.AuthenticationProperties.BgImage,
                            context.Properties.Items[Constants.AuthenticationProperties.BgImage]);
                    }

                    if (context.Properties.Items.ContainsKey(Constants.AuthenticationProperties.LogoImage))
                    {
                        context.ProtocolMessage.SetParameter(Constants.AuthenticationProperties.LogoImage,
                            context.Properties.Items[Constants.AuthenticationProperties.LogoImage]);
                    }

                    if (context.Properties.Items.ContainsKey(Constants.AuthenticationProperties.IdTokenHint))
                    {
                        context.ProtocolMessage.SetParameter(Constants.AuthenticationProperties.IdTokenHint,
                            context.Properties.Items[Constants.AuthenticationProperties.IdTokenHint]);
                    }

                    var policyClaims = new List<Claim>();

                    if (context.Properties.Items.ContainsKey(Constants.AuthenticationProperties.InvitedEmail))
                    {
                        policyClaims.Add(new Claim(Constants.AuthenticationProperties.InvitedEmail,
                            context.Properties.Items[Constants.AuthenticationProperties.InvitedEmail]));
                    }

                    if (context.Properties.Items.ContainsKey(Constants.AuthenticationProperties.InvitedAccountId))
                    {
                        policyClaims.Add(new Claim(Constants.AuthenticationProperties.InvitedAccountId,
                            context.Properties.Items[Constants.AuthenticationProperties.InvitedAccountId]));
                    }

                    if (context.Properties.Items.ContainsKey(Constants.AuthenticationProperties.InvitedGroupId))
                    {
                        policyClaims.Add(new Claim(Constants.AuthenticationProperties.InvitedGroupId,
                            context.Properties.Items[Constants.AuthenticationProperties.InvitedGroupId]));
                    }

                    if (policyClaims.Any())
                    {
                        TimeSpan policyTokenLifetime;

                        // Get the lifetime of the JSON Web Token (JWT) from the authentication session...
                        if (!context.Properties.Items.ContainsKey("policy_token_lifetime") ||
                            !TimeSpan.TryParse(context.Properties.Items["policy_token_lifetime"], out policyTokenLifetime))
                        {
                            // ... Or set it to a default time of 5 minutes.
                            policyTokenLifetime = new TimeSpan(0, 0, 5, 0);
                        }

                        var selfIssuedToken = CreateSelfIssuedToken(
                            configuration.Issuer,
                            context.ProtocolMessage.RedirectUri,
                            policyTokenLifetime,
                            context.Options.ClientSecret,
                            policyClaims);

                        context.ProtocolMessage.Parameters.Add("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer");
                        context.ProtocolMessage.Parameters.Add("client_assertion", selfIssuedToken);
                    }
                },
                OnRedirectToIdentityProviderForSignOut = async context =>
                {
                    var policy = context.Properties.Items.ContainsKey(Constants.AuthenticationProperties.Policy)
                        ? context.Properties.Items[Constants.AuthenticationProperties.Policy]
                        : policyManager.SignUpOrSignInWithPersonalAccountLocalEmailAndSocial;
                    var configuration = await GetB2COpenIdConnectConfigurationAsync(context, policy);
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

                    var claims = new List<Claim> { new Claim(ClaimTypes.Role, roleClaimValue) };

                    var identity = new ClaimsIdentity(claims);
                    context.Principal.AddIdentity(identity);
                    return Task.CompletedTask;
                }
            };
        }


        private OpenIdConnectEvents CreateB2BOpenIdConnectEvents()
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
                        ownerIdClaimValue = identityProvider.StartsWith("https://sts.windows.net/")
                            ? identityProvider
                            : $"{identityProvider}/{context.Principal.FindFirstValue(Constants.ClaimTypes.ObjectIdentifier)}";
                        roleClaimValue = Constants.Roles.Partner;
                    }
                    else
                    {
                        ownerIdClaimValue = context.Principal.FindFirstValue(Constants.ClaimTypes.Issuer);
                        roleClaimValue = Constants.Roles.Employee;
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(Constants.ClaimTypes.OwnerIdentifier, ownerIdClaimValue), new Claim(ClaimTypes.Role, roleClaimValue)
                    };

                    var identity = new ClaimsIdentity(claims);
                    context.Principal.AddIdentity(identity);
                    return Task.CompletedTask;
                }
            };
        }

        private OpenIdConnectEvents CreateBetaAppOidcEvents()
        {
            var authenticationOptions = AuthenticationBetaAppAccessOptions.Construct(Configuration);

            return new OpenIdConnectEvents
            {
                OnRemoteFailure = context =>
                {
                    if (context.Failure.Message == "Correlation failed.")
                    {
                        //// [ log error ]
                        context.HandleResponse();
                        //// redirect to some help page or handle it as you wish
                        context.Response.Redirect("/");
                    }

                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    context.Fail(context.Exception);
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    if (context.ProtocolMessage.Parameters.Any(x => x.Key == "response"))
                    {
                        var jsonResponse = JsonConvert.DeserializeObject<Constants.JsonResponse>(context.ProtocolMessage.Parameters["response"]);

                        if (jsonResponse != null && !string.IsNullOrEmpty(jsonResponse.response) && jsonResponse.response.Contains("B2C_V1_90001"))
                        {
                            context.Response.Redirect("/Account/AgeGatingError");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }
                    }

                    if (!string.IsNullOrEmpty(context.ProtocolMessage.Error) && !string.IsNullOrEmpty(context.ProtocolMessage.ErrorDescription))
                    {
                        if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C90091"))
                        {
                            var policy = context.Properties.Items[Constants.AuthenticationProperties.Policy];

                            //if (policy == Constants.Policies.PasswordReset ||
                            //    policy == Constants.Policies.SignUpOrSignInWithPersonalAccountLocalEmailAndSocial)
                            //{
                            //    var command =
                            //        $"{Constants.AuthenticationSchemes.CustomerAuth}:{Constants.Policies.SignUpOrSignInWithPersonalAccountLocalEmailAndSocial}";

                            //    var uiLocale = string.Empty;

                            //    if (context.Properties.Items.Any(x => x.Key == Constants.AuthenticationProperties.UILocales))
                            //    {
                            //        uiLocale = context.Properties.Items[Constants.AuthenticationProperties.UILocales];
                            //    }

                            //    context.Response.Redirect($"/Account/LogInFor?command={command}&uiLocale={uiLocale}");
                            //    context.HandleResponse();
                            //}
                            //else
                            //{
                            //    context.Response.Redirect("/");
                            //    context.HandleResponse();
                            //}
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C90118"))
                        {
                            var uiLocale = string.Empty;

                            if (context.Properties.Items.Any(x => x.Key == Constants.AuthenticationProperties.UILocales))
                            {
                                uiLocale = context.Properties.Items[Constants.AuthenticationProperties.UILocales];
                            }

                            context.Response.Redirect($"/Account/ResetPassword?uiLocale={uiLocale}");
                            context.HandleResponse();
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C99001"))
                        {
                            context.Response.Redirect($"/Account/LinkError??ReturnUrl={context.Properties.RedirectUri}");
                            context.HandleResponse();
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C99002"))
                        {
                            context.Response.Redirect("/Account/LogOut");
                            context.HandleResponse();
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C90157"))
                        {
                            context.Response.Redirect("/Account/RetryExceededError");
                            context.HandleResponse();
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C90037"))
                        {
                            context.Response.Redirect("/Account/AgeGatingError");
                            context.HandleResponse();
                        }
                        else if (context.ProtocolMessage.ErrorDescription.StartsWith("AADB2C90273"))
                        {
                            context.Response.Redirect("/");
                            context.HandleResponse();
                        }
                    }

                    return Task.CompletedTask;
                },
                OnRedirectToIdentityProvider = async context =>
                {
                    var configuration = await GetB2COpenIdConnectConfigurationAsync(context, authenticationOptions.Policy);
                    context.ProtocolMessage.IssuerAddress = configuration.AuthorizationEndpoint;
                },
                OnRedirectToIdentityProviderForSignOut = async context =>
                {
                    var configuration = await GetB2COpenIdConnectConfigurationAsync(context, authenticationOptions.Policy);
                    context.ProtocolMessage.IssuerAddress = configuration.EndSessionEndpoint;
                },
                OnTokenValidated = context =>
                {
                    //var requestedPolicy = context.Properties.Items[Constants.AuthenticationProperties.Policy];
                    //var issuedPolicy = context.Principal.FindFirstValue(Constants.ClaimTypes.TrustFrameworkPolicy);

                    //if (!string.Equals(issuedPolicy, requestedPolicy, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    context.Fail($"Access denied: The issued policy '{issuedPolicy}' is different to the requested policy '{requestedPolicy}'.");
                    //    return Task.CompletedTask;
                    //}

                    //string roleClaimValue;
                    //var identityProvider = context.Principal.FindFirstValue(Constants.ClaimTypes.IdentityProvider);

                    //if (identityProvider != null && identityProvider.StartsWith("https://sts.windows.net/"))
                    //{
                    //    var businessCustomerRole = context.Principal.FindFirstValue(Constants.ClaimTypes.BusinessCustomerRole);

                    //    if (businessCustomerRole == "Manager")
                    //    {
                    //        roleClaimValue = Constants.Roles.BusinessCustomerManager;
                    //    }
                    //    else
                    //    {
                    //        roleClaimValue = Constants.Roles.BusinessCustomerStocker;
                    //    }
                    //}
                    //else
                    //{
                    //    roleClaimValue = Constants.Roles.IndividualCustomer;
                    //}

                    //var claims = new List<Claim>
                    //{
                    //    new Claim(ClaimTypes.Role, roleClaimValue)
                    //};

                    //var identity = new ClaimsIdentity(claims);
                    //context.Principal.AddIdentity(identity);
                    return Task.CompletedTask;
                }
            };
        }

        private Task<OpenIdConnectConfiguration> GetB2COpenIdConnectConfigurationAsync(RedirectContext context, string policy)
        {
            var configurationManager = (PolicyConfigurationManager)context.Options.ConfigurationManager;
            return configurationManager.GetConfigurationForPolicyAsync(policy, CancellationToken.None);
        }

        internal string CreateSelfIssuedToken(
            string issuer,
            string audience,
            TimeSpan expiration,
            string signingSecret,
            ICollection<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var nowUtc = DateTime.UtcNow;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingSecret));
            var signingCredentials = new SigningCredentials(key, "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = audience,
                Expires = nowUtc.Add(expiration),
                IssuedAt = nowUtc,
                Issuer = issuer,
                NotBefore = nowUtc,
                SigningCredentials = signingCredentials,
                Subject = new ClaimsIdentity(claims)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}