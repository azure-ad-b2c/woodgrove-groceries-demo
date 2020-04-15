namespace WoodGroveGroceriesWebApplication.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Extensions;
    using Managers;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Models.Settings;
    using ViewServices;
    using static Managers.IndustryManager;

    public class AccountController : BaseController
    {
        private readonly PolicyManager _policyManager;
        private readonly ITrolleyManager _trolleyManager;
        private readonly ITrolleyViewService _trolleyViewService;

        private readonly IConfiguration Config;
        private readonly IdentityService identityService;

        public AccountController(
            ITrolleyViewService trolleyViewService,
            ITrolleyManager trolleyManager,
            IConfiguration config,
            IdentityService identityService,
            PolicyManager policyManager,
            IndustryManager industryManager) : base(industryManager)
        {
            _trolleyViewService = trolleyViewService ?? throw new ArgumentNullException(nameof(trolleyViewService));
            _trolleyManager = trolleyManager ?? throw new ArgumentNullException(nameof(trolleyManager));
            this.identityService = identityService;
            _policyManager = policyManager;

            Config = config;
        }

        public IActionResult EditProfile()
        {
            var policy = User.IsInBusinessCustomerManagerRole() || User.IsInBusinessCustomerStockerRole()
                ? _policyManager.ProfileUpdateWithWorkAccount
                : _policyManager.ProfileUpdateWithPersonalAccount;

            var uiLocale = GetCustomLocalization();

            var industry = _industryManager.GetIndustry();
            var background = Request.Cookies[Constants.DemoCookies.BgImageKey];
            if (string.IsNullOrEmpty(background))
            {
                background = $"{Request.Scheme}://{Request.Host}/{industry.DefaultCustomerAuthBackground}".ToBase64Encode();
            }

            return new ChallengeResult(
                Constants.AuthenticationSchemes.CustomerAuth,
                new AuthenticationProperties(
                    new Dictionary<string, string>
                    {
                        {Constants.AuthenticationProperties.UILocales, uiLocale},
                        {Constants.AuthenticationProperties.Policy, policy},
                        {Constants.AuthenticationProperties.BgImage, background},
                        {Constants.AuthenticationProperties.LogoImage, Request.Cookies[Constants.DemoCookies.LogoImageKey]}
                    })
                { RedirectUri = Url.Action("LoggedIn", "Account", null, Request.Scheme) });
        }

        [Authorize(Policy = Constants.AuthorizationPolicies.ChangeUserRole)]
        public IActionResult ChangeUserRole()
        {
            var policy = User.IsInBusinessCustomerManagerRole() ? _policyManager.SetStockerRole : User.IsInBusinessCustomerStockerRole() ? _policyManager.SetManagerRole : null;

            var uiLocale = GetCustomLocalization();

            var industry = _industryManager.GetIndustry();
            var background = Request.Cookies[Constants.DemoCookies.BgImageKey];
            if (string.IsNullOrEmpty(background))
            {
                background = $"{Request.Scheme}://{Request.Host}/{industry.DefaultCustomerAuthBackground}".ToBase64Encode();
            }

            return new ChallengeResult(
                Constants.AuthenticationSchemes.CustomerAuth,
                new AuthenticationProperties(
                    new Dictionary<string, string>
                    {
                        {Constants.AuthenticationProperties.UILocales, uiLocale},
                        {Constants.AuthenticationProperties.Policy, policy},
                        {Constants.AuthenticationProperties.BgImage, background},
                        {Constants.AuthenticationProperties.LogoImage, Request.Cookies[Constants.DemoCookies.LogoImageKey]}
                    })
                { RedirectUri = Url.Action("LoggedIn", "Account", null, Request.Scheme) });
        }

        public IActionResult InviteExternalUser()
        {
            var policy = _policyManager.InviteExternalUser;
            var uiLocale = GetCustomLocalization();

            var industry = _industryManager.GetIndustry();
            var background = Request.Cookies[Constants.DemoCookies.BgImageKey];
            if (string.IsNullOrEmpty(background))
            {
                background = $"{Request.Scheme}://{Request.Host}/{industry.DefaultCustomerAuthBackground}".ToBase64Encode();
            }

            return new ChallengeResult(
                Constants.AuthenticationSchemes.CustomerAuth,
                new AuthenticationProperties(
                    new Dictionary<string, string>
                    {
                        {Constants.AuthenticationProperties.UILocales, uiLocale},
                        {Constants.AuthenticationProperties.Policy, policy},
                        {Constants.AuthenticationProperties.BgImage, background},
                        {Constants.AuthenticationProperties.LogoImage, Request.Cookies[Constants.DemoCookies.LogoImageKey]}
                    })
                { RedirectUri = Url.Action("LoggedIn", "Account", null, Request.Scheme) });
        }

        [Authorize(Policy = Constants.AuthorizationPolicies.BetaAppAccess)]
        [HttpGet]
        public IActionResult LogIn()
        {
            var uilocale = Request.Cookies[Constants.DemoCookies.UILocale].ToBase64Decode();
            ViewBag.UILocale = uilocale ?? string.Empty;
            return View();
        }

        [HttpGet]
        [HttpPost]
        public IActionResult LogInFor(string command, string uiLocale)
        {
            if (string.IsNullOrEmpty(uiLocale))
            {
                uiLocale = "en";
            }

            CreateCookie(Constants.DemoCookies.UILocale, uiLocale.ToBase64Encode());

            uiLocale = _industryManager.GetIndustry()?.GetCustomLocaleReplacement(uiLocale);

            switch (command)
            {
                case "customerSignIn":
                default:
                    return LogInForIndividualCustomer(uiLocale);

                case "customerSignUp":
                    return LogInForIndividualCustomer(uiLocale, true);

                case "businessSignIn":
                case "businessSignUp":
                    return LogInForBusinessCustomer(uiLocale);

                case "partnerSignIn":
                    return LogInForPartner(uiLocale);

                case "partnerSignUp":
                    return LogInForPartner(uiLocale, true);
            }
        }

        private IActionResult LogInFor(string authenticationScheme, string policy, string uiLocale, bool isSignUp = false)
        {
            if (!identityService.IsUserLoggedIn(User))
            {
                var authenticationOptions = AuthenticationPartnerOptions.Construct(Config);

                var authProperties = new Dictionary<string, string>
                {
                    {Constants.AuthenticationProperties.Policy, policy}, {Constants.AuthenticationProperties.UILocales, uiLocale}
                };

                if (authenticationScheme == Constants.AuthenticationSchemes.CustomerAuth)
                {
                    var industry = _industryManager.GetIndustry();

                    var background = Request.Cookies[Constants.DemoCookies.BgImageKey];

                    if (string.IsNullOrEmpty(background))
                    {
                        background = $"{Request.Scheme}://{Request.Host}/{industry.DefaultCustomerAuthBackground}".ToBase64Encode();
                    }

                    authProperties.Add(Constants.AuthenticationProperties.BgImage, background);
                    authProperties.Add(Constants.AuthenticationProperties.LogoImage, Request.Cookies[Constants.DemoCookies.LogoImageKey]);
                }

                if (authenticationScheme == Constants.AuthenticationSchemes.BusinessCustomerAuth && isSignUp)
                {
                    return new RedirectResult("https://aka.ms/woodgrovesuppliersignup");
                }

                return new ChallengeResult(
                    authenticationScheme,
                    new AuthenticationProperties(new Dictionary<string, string>(authProperties))
                    {
                        RedirectUri = Url.Action("LoggedIn", "Account", null, Request.Scheme)
                    });
            }

            return RedirectToHome();
        }

        public IActionResult LoggedIn()
        {
            if (User.IsInBusinessCustomerManagerRole() || User.IsInIndividualCustomerRole() || User.IsInEmployeeRole() || User.IsInPartnerRole())
            {
                return RedirectToAction("Index", "CatalogItem");
            }

            return RedirectToAction("Index", "Pantry");
        }

        public IActionResult LogInForBusinessCustomer(string uiLocale)
        {
            return LogInFor(Constants.AuthenticationSchemes.CustomerAuth, _policyManager.SignUpOrSignInWithWorkAccount, uiLocale);
        }

        public IActionResult LogInForIndividualCustomer(string uiLocale, bool isSignUp = false)
        {
            var authenticationOptions = AuthenticationPartnerOptions.Construct(Config);

            var configuredPolicy = Request.Cookies[Constants.DemoCookies.DefaultSigninPolicyKey].ToBase64Decode();

            if (!_policyManager.PolicyList.ContainsValue(configuredPolicy))
            {
                configuredPolicy = _policyManager.DefaultSignInPolicy;
            }

            var signInPolicy = string.IsNullOrEmpty(configuredPolicy)
                ? _policyManager.SignUpOrSignInWithPersonalAccountLocalEmailAndSocial
                : configuredPolicy;

            var signUpPolicy = signInPolicy == _policyManager.SignInWithPersonalAccountLocalPhoneWithOtp
                ? _policyManager.SignUpWithPersonalAccountLocalPhoneWithOtp
                : signInPolicy;

            return LogInFor(Constants.AuthenticationSchemes.CustomerAuth, isSignUp ? signUpPolicy : signInPolicy, uiLocale);
        }

        public IActionResult LogInForPartner(string uiLocale, bool isSignUp = false)
        {
            var options = AuthenticationPartnerOptions.Construct(Config);

            switch (options.Experience)
            {
                case PartnerExperience.B2BSelfSignup:
                    return LogInFor(Constants.AuthenticationSchemes.PartnerOpenIdConnect, null, uiLocale);

                case PartnerExperience.IGLM:
                default:
                    return LogInFor(Constants.AuthenticationSchemes.PartnerOpenIdConnect, null, uiLocale, isSignUp);
            }
        }

        public async Task<IActionResult> LogOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignOutAsync(
                    authenticateResult.Properties.Items[".AuthScheme"],
                    new AuthenticationProperties(
                        new Dictionary<string, string>
                        {
                            {Constants.AuthenticationProperties.Policy, User.FindFirstValue(Constants.ClaimTypes.TrustFrameworkPolicy)}
                        })
                    { RedirectUri = Url.Action("LoggedOut", "Account", null, Request.Scheme) });

                return new EmptyResult();
            }

            return RedirectToHome();
        }

        public IActionResult UserDeleted()
        {
            return RedirectToAction("LogOut", "Account");
        }

        public IActionResult DeleteAccount()
        {
            var uiLocale = GetCustomLocalization();

            var policy = _policyManager.DeleteAccount;

            var industry = _industryManager.GetIndustry();
            var background = Request.Cookies[Constants.DemoCookies.BgImageKey];
            if (string.IsNullOrEmpty(background))
            {
                background = $"{Request.Scheme}://{Request.Host}/{industry.DefaultCustomerAuthBackground}".ToBase64Encode();
            }

            return new ChallengeResult(
                Constants.AuthenticationSchemes.CustomerAuth,
                new AuthenticationProperties(
                    new Dictionary<string, string>
                    {
                        {Constants.AuthenticationProperties.Policy, policy},
                        {Constants.AuthenticationProperties.UILocales, uiLocale},
                        {Constants.AuthenticationProperties.BgImage, background},
                        {Constants.AuthenticationProperties.LogoImage, Request.Cookies[Constants.DemoCookies.LogoImageKey]}
                    })
                { RedirectUri = Url.Action("LogOut", "Account", null, Request.Scheme) });
        }

        public async Task<IActionResult> LoggedOut()
        {
            if (!identityService.IsUserLoggedIn(User))
            {
                var userId = User.FindFirstValue(Constants.ClaimTypes.ObjectIdentifier);
                var trolleyViewModel = await _trolleyViewService.GetOrCreateTrolleyForOwnerAsync(userId);

                if (trolleyViewModel == null || !trolleyViewModel.Items.Any())
                {
                    return RedirectToAction("Index", "Home");
                }

                await _trolleyManager.RemoveAllItemFromTrolleyAsync(trolleyViewModel.Id);
            }

            return RedirectToHome();
        }

        public IActionResult ResetPassword()
        {
            var uiLocale = GetCustomLocalization();

            var industry = _industryManager.GetIndustry();
            var background = Request.Cookies[Constants.DemoCookies.BgImageKey];
            if (string.IsNullOrEmpty(background))
            {
                background = $"{Request.Scheme}://{Request.Host}/{industry.DefaultCustomerAuthBackground}".ToBase64Encode();
            }

            return new ChallengeResult(
                Constants.AuthenticationSchemes.CustomerAuth,
                new AuthenticationProperties(
                    new Dictionary<string, string>
                    {
                        {Constants.AuthenticationProperties.Policy, _policyManager.PasswordReset},
                        {Constants.AuthenticationProperties.UILocales, uiLocale},
                        {Constants.AuthenticationProperties.BgImage, background},
                        {Constants.AuthenticationProperties.LogoImage, Request.Cookies[Constants.DemoCookies.LogoImageKey]}
                    })
                { RedirectUri = Url.Action("LoggedIn", "Account", null, Request.Scheme) });
        }

        public IActionResult Mfa()
        {
            var uiLocale = GetCustomLocalization();

            if (identityService.IsUserLoggedIn(User))
            {
                var mfaTypeClaim = User.Claims.FirstOrDefault(c => c.Type == "mfaType")?.Value;

                var policy = mfaTypeClaim != null && mfaTypeClaim.Equals("totp")
                    ? _policyManager.StepUpTotp
                    : _policyManager.MultifactorAuthentication;

                var industry = _industryManager.GetIndustry();
                var background = Request.Cookies[Constants.DemoCookies.BgImageKey];
                if (string.IsNullOrEmpty(background))
                {
                    background = $"{Request.Scheme}://{Request.Host}/{industry.DefaultCustomerAuthBackground}".ToBase64Encode();
                }

                return new ChallengeResult(
                    Constants.AuthenticationSchemes.CustomerAuth,
                    new AuthenticationProperties(
                        new Dictionary<string, string>
                        {
                            {Constants.AuthenticationProperties.Policy, policy},
                            {Constants.AuthenticationProperties.UILocales, uiLocale},
                            {Constants.AuthenticationProperties.BgImage, background},
                            {Constants.AuthenticationProperties.LogoImage, Request.Cookies[Constants.DemoCookies.LogoImageKey]}
                        })
                    { RedirectUri = Url.Action("Index", "Checkout", null, Request.Scheme) });
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult LinkAccount()
        {
            if (identityService.IsUserLoggedIn(User))
            {
                var policy = _policyManager.LinkWithSocialAccounts;
                var uiLocale = GetCustomLocalization();

                var industry = _industryManager.GetIndustry();
                var background = Request.Cookies[Constants.DemoCookies.BgImageKey];
                if (string.IsNullOrEmpty(background))
                {
                    background = $"{Request.Scheme}://{Request.Host}/{industry.DefaultCustomerAuthBackground}".ToBase64Encode();
                }

                return new ChallengeResult(
                    Constants.AuthenticationSchemes.CustomerAuth,
                    new AuthenticationProperties(
                        new Dictionary<string, string>
                        {
                            {Constants.AuthenticationProperties.Policy, policy},
                            {Constants.AuthenticationProperties.UILocales, uiLocale},
                            {Constants.AuthenticationProperties.BgImage, background},
                            {Constants.AuthenticationProperties.LogoImage, Request.Cookies[Constants.DemoCookies.LogoImageKey]}
                        })
                    { RedirectUri = Url.Action("LoggedIn", "Account", null, Request.Scheme) });
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult AcceptInvitation(string token, string logo, string bkg, string locale)
        {
            if (!identityService.IsUserLoggedIn(User))
            {
                if (!string.IsNullOrEmpty(locale))
                {
                    var industryIdentifier = _industryManager.GetIndustryIdentifier(locale);

                    if (industryIdentifier != Industry.None)
                    {
                        CreateCookie(Constants.DemoCookies.IndustryKey, industryIdentifier.ToString().ToBase64Encode());
                    }
                }

                if (!string.IsNullOrEmpty(logo))
                {
                    CreateCookie(Constants.DemoCookies.LogoImageKey, logo);
                }

                if (!string.IsNullOrEmpty(bkg))
                {
                    CreateCookie(Constants.DemoCookies.BgImageKey, logo);
                }

                var industry = _industryManager.GetIndustry();
                var background = Request.Cookies[Constants.DemoCookies.BgImageKey];
                if (string.IsNullOrEmpty(background))
                {
                    background = $"{Request.Scheme}://{Request.Host}/{industry.DefaultCustomerAuthBackground}".ToBase64Encode();
                }

                return new ChallengeResult(
                    Constants.AuthenticationSchemes.CustomerAuth,
                    new AuthenticationProperties(
                        new Dictionary<string, string>
                        {
                            {Constants.AuthenticationProperties.Policy, _policyManager.SignUpWithPersonalAccountLocalEmailAcceptInvitation},
                            {Constants.AuthenticationProperties.UILocales, locale},
                            {Constants.AuthenticationProperties.BgImage, bkg ?? background},
                            {Constants.AuthenticationProperties.LogoImage, logo ?? Request.Cookies[Constants.DemoCookies.LogoImageKey]},
                            {Constants.AuthenticationProperties.IdTokenHint, token}
                        })
                    { RedirectUri = Url.Action("LoggedIn", "Account", null, Request.Scheme) });
            }

            return RedirectToHome();
        }

        public IActionResult ForgotUsername()
        {
            var policy = _policyManager.ForgotUserName;
            var uiLocale = GetCustomLocalization();

            var industry = _industryManager.GetIndustry();
            var background = Request.Cookies[Constants.DemoCookies.BgImageKey];
            if (string.IsNullOrEmpty(background))
            {
                background = $"{Request.Scheme}://{Request.Host}/{industry.DefaultCustomerAuthBackground}".ToBase64Encode();
            }

            return new ChallengeResult(
                Constants.AuthenticationSchemes.CustomerAuth,
                new AuthenticationProperties(
                    new Dictionary<string, string>
                    {
                        {Constants.AuthenticationProperties.Policy, policy},
                        {Constants.AuthenticationProperties.UILocales, uiLocale},
                        {Constants.AuthenticationProperties.BgImage, background},
                        {Constants.AuthenticationProperties.LogoImage, Request.Cookies[Constants.DemoCookies.LogoImageKey]}
                    })
                { RedirectUri = Url.Action("LoggedIn", "Account", null, Request.Scheme) });
        }

        public async Task<IActionResult> LinkError(string returnUrl)
        {
            if (identityService.IsUserLoggedIn(User))
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignOutAsync(
                    authenticateResult.Properties.Items[".AuthScheme"],
                    new AuthenticationProperties(
                        new Dictionary<string, string>
                        {
                            {Constants.AuthenticationProperties.Policy, User.FindFirstValue(Constants.ClaimTypes.TrustFrameworkPolicy)}
                        })
                    { RedirectUri = Url.Action("LinkError", "Account", null, Request.Scheme) });

                return new EmptyResult();
            }

            return View();
        }

        public IActionResult AgeGatingError()
        {
            return View();
        }

        public IActionResult TimeoutError()
        {
            return View();
        }

        public IActionResult RetryExceededError()
        {
            return View();
        }

        private IActionResult RedirectToHome()
        {
            return RedirectToAction("Index", "Home");
        }

        private string GetCustomLocalization()
        {
            var uiLocale = Request.Cookies[Constants.DemoCookies.UILocale].ToBase64Decode();

            if (string.IsNullOrEmpty(uiLocale))
            {
                return uiLocale;
            }

            return _industryManager.GetIndustry()?.GetCustomLocaleReplacement(uiLocale);
        }
    }
}