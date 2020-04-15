namespace WoodGroveGroceriesWebApplication.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Managers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using Models;
    using Models.Settings;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using Services;

    [Produces("application/json")]
    public class MailServiceController : Controller
    {
        private static Lazy<X509SigningCredentials> SigningCredentials;
        private readonly PolicyManager _policyManager;

        private readonly AppSettingsModel AppSettings;
        private readonly IConfiguration Config;

        private readonly GraphSettingsModel graphSettingsModel;

        private readonly IndustryManager industryManager;

        public MailServiceController(IConfiguration config, PolicyManager policyManager, IndustryManager industryManager)
        {
            Config = config;
            _policyManager = policyManager;
            AppSettings = config.GetSection("AppSettings")
                .Get<AppSettingsModel>();

            graphSettingsModel = config.GetSection("GraphApi")
                .Get<GraphSettingsModel>();

            this.industryManager = industryManager;

            SigningCredentials = new Lazy<X509SigningCredentials>(() =>
            {
                var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadOnly);
                var certCollection = certStore.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    AppSettings.SigningCertThumbprint,
                    false);
                // Get the first cert with the thumb-print
                if (certCollection.Count > 0)
                {
                    return new X509SigningCredentials(certCollection[0]);
                }

                throw new Exception("Certificate not found");
            });
        }

        [Route("api/MailService/MakeInviteUrl")]
        public IActionResult MakeInviteUrl([FromBody] LinkGenerationParametersModel inputClaims)
        {
            if (inputClaims == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new B2CResponseContent("Can not deserialize input claims", HttpStatusCode.BadRequest));
            }

            var token = BuildIdToken(inputClaims.ToAddress, inputClaims.AccountNumber, inputClaims.GroupName);
            var link = BuildUrl(token, inputClaims.AppLogo, inputClaims.AppBackground, inputClaims.Culture);

            return StatusCode(StatusCodes.Status200OK, new {Url = link});
        }

        [Route("api/MailService/SendUsernameToEmail")]
        public async Task<IActionResult> SendUsernameToEmail([FromBody] SendUsernameInputModel inputClaims)
        {
            if (inputClaims == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new B2CResponseContent("Can not deserialize input claims", HttpStatusCode.BadRequest));
            }

            try
            {
                var b2CGraphClient = new B2CGraphClientService(graphSettingsModel.Tenant, graphSettingsModel.ClientId,
                    graphSettingsModel.ClientSecret, graphSettingsModel.ExtensionAppId);
                var JSON = await b2CGraphClient.SearchUserByAccountNumber(inputClaims.AccountNumber);
                JSON = JSON.Replace($"extension_{graphSettingsModel.ExtensionAppId}_PhoneNumber", "phoneNumber");
                var result = GraphUsersModel.Parse(JSON);

                var users = result.value
                    .Where(x => !string.IsNullOrEmpty(x.phoneNumber) && x.phoneNumber.ToLower() == inputClaims.PhoneNumber.ToLower() &&
                                x.signInNames.Any(y => y.type == "userName") && x.otherMails.Any()).Select(x =>
                        new {UserName = x.signInNames.First(y => y.type == "userName"), Email = x.otherMails.First()}).GroupBy(x => x.Email).ToList();

                foreach (var user in users)
                {
                    var userNames = user.Select(x => x.UserName.value).ToList();

                    //var htmlContent = "<p>Hi,</p><p>Please find the {usernameplural} associated with the account number '{accountNumber}' <br/><br/> {userNameList}</p><p>Regards,</p><p>The WoodGrove Groceries Team</p>";

                    var customLanguage = industryManager.GetIndustry(inputClaims.Culture)
                        ?.GetLocalizedEmailString(LocalizedEmailUse.SendUsernames, inputClaims.Culture);

                    if (customLanguage == null)
                    {
                        return StatusCode(StatusCodes.Status400BadRequest,
                            new B2CResponseContent("Unable to find the localization language", HttpStatusCode.BadRequest));
                    }

                    var replacements = new Dictionary<string, string>();
                    replacements.Add("{usernameplural}", "username" + (userNames.Count == 1 ? string.Empty : "s"));
                    replacements.Add("{accountNumber}", inputClaims.AccountNumber);
                    replacements.Add("{userNameList}", string.Join(" < br/>", user.Select(x => x.UserName.value).ToList()));

                    foreach (var kvp in replacements)
                    {
                        customLanguage.EmailCodeHtmlContent = customLanguage.EmailCodeHtmlContent.Replace(kvp.Key, kvp.Value);
                    }

                    //var subject = "Welcome to WoodGrove Groceries";
                    var subject = customLanguage.EmailCodeSubject;
                    var client = new SendGridClient(Config["SendGrid:ApiKey"]);
                    var from = new EmailAddress(inputClaims.FromAddress);
                    var to = new EmailAddress(user.Key);
                    var plainTextContent = Regex.Replace(customLanguage.EmailCodeHtmlContent, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, customLanguage.EmailCodeHtmlContent);
                    //var plainTextContent = Regex.Replace(htmlContent, "<[^>]*>", "");
                    //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                    try
                    {
                        var response = await client.SendEmailAsync(msg);
                    }
                    catch
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new B2CResponseContent("Failed to send email", HttpStatusCode.InternalServerError));
                    }
                }

                return Ok(new B2CResponseContent("Success", HttpStatusCode.OK));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new B2CResponseContent("Failed to send email", HttpStatusCode.InternalServerError));
            }
        }

        private string BuildUrl(string token, string appLogo, string appBackground, string culture)
        {
            var url = $"{Url.Action("AcceptInvitation", "Account", null, Request.Scheme)}?token=" + token;

            if (!string.IsNullOrEmpty(appLogo))
            {
                url += $"&logo={appLogo}";
            }

            if (!string.IsNullOrEmpty(appBackground))
            {
                url += $"&bkg={appBackground}";
            }

            if (!string.IsNullOrEmpty(culture))
            {
                url += $"&locale={culture}";
            }

            return url;
        }

        private string BuildIdToken(string invitedEmail, string invitedAccountId, string invitedGroupId)
        {
            var configOptions = AuthenticationCustomerOptions.Construct(Config);

            var issuer = $"{Request.Scheme}://{Request.Host}{Request.PathBase.Value}/";

            // All parameters send to Azure AD B2C needs to be sent as claims
            IList<Claim> claims = new List<Claim>();
            claims.Add(new Claim(Constants.AuthenticationProperties.InvitedEmail, invitedEmail, ClaimValueTypes.String, issuer));
            claims.Add(new Claim(Constants.AuthenticationProperties.InvitedAccountId, invitedAccountId, ClaimValueTypes.String, issuer));
            claims.Add(new Claim(Constants.AuthenticationProperties.InvitedGroupId, invitedGroupId, ClaimValueTypes.String, issuer));

            // Create the token
            var token = new JwtSecurityToken(
                issuer,
                configOptions.ClientId,
                claims,
                DateTime.Now,
                DateTime.Now.AddDays(7),
                SigningCredentials.Value);

            // Get the representation of the signed token
            var jwtHandler = new JwtSecurityTokenHandler();

            return jwtHandler.WriteToken(token);
        }
    }
}