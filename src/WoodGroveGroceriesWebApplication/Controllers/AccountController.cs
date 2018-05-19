using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WoodGroveGroceriesWebApplication.Extensions;

namespace WoodGroveGroceriesWebApplication.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult EditProfile()
        {
            var policy = User.IsInBusinessCustomerManagerRole() || User.IsInBusinessCustomerStockerRole() ? Constants.Policies.ProfileUpdateWithWorkAccount : Constants.Policies.ProfileUpdateWithPersonalAccount;

            return new ChallengeResult(
                Constants.AuthenticationSchemes.B2COpenIdConnect,
                new AuthenticationProperties(
                    new Dictionary<string, string>
                    {
                        {Constants.AuthenticationProperties.Policy, policy}
                    })
                {
                    RedirectUri = "/"
                });
        }

        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }

        [HttpGet]
        [HttpPost]
        public IActionResult LogInFor(string command, string uiLocale)
        {
            var commandParts = command.Split(':');
            return LogInFor(commandParts[0], commandParts[1], uiLocale);
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
            return LogInFor(Constants.AuthenticationSchemes.B2COpenIdConnect, Constants.Policies.SignUpOrSignInWithWorkAccount, uiLocale);
        }

        public IActionResult LogInForIndividualCustomer(string uiLocale)
        {
            return LogInFor(Constants.AuthenticationSchemes.B2COpenIdConnect, Constants.Policies.SignUpOrSignInWithPersonalAccount, uiLocale);
        }

        public IActionResult LogInForPartner(string uiLocale)
        {
            return LogInFor(Constants.AuthenticationSchemes.B2BOpenIdConnect, null, uiLocale);
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
                    {
                        RedirectUri = Url.Action("Index", "Home", values: null, protocol: Request.Scheme)
                    });

                return new EmptyResult();
            }

            return RedirectToHome();
        }

        public IActionResult ResetPassword(string uiLocale)
        {
            return new ChallengeResult(
                Constants.AuthenticationSchemes.B2COpenIdConnect,
                new AuthenticationProperties(
                    new Dictionary<string, string>
                    {
                        {Constants.AuthenticationProperties.Policy, Constants.Policies.PasswordReset},
                        {Constants.AuthenticationProperties.UILocales, uiLocale}
                    })
                {
                    RedirectUri = "/"
                });
        }

        private IActionResult LogInFor(string authenticationScheme, string policy, string uiLocale)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(
                    authenticationScheme,
                    new AuthenticationProperties(
                        new Dictionary<string, string>
                        {
                            {Constants.AuthenticationProperties.Policy, policy},
                            {Constants.AuthenticationProperties.UILocales, uiLocale}
                        })
                    {
                        RedirectUri = Url.Action("LoggedIn", "Account", values: null, protocol: Request.Scheme)
                    });
            }

            return RedirectToHome();
        }

        private IActionResult RedirectToHome()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
