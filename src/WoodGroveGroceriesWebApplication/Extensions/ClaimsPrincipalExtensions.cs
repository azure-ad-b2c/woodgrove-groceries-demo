namespace WoodGroveGroceriesWebApplication.Extensions
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using Managers;
    using Microsoft.Extensions.Configuration;
    using Models.Settings;
    using Newtonsoft.Json;

    public static class ClaimsPrincipalExtensions
    {
        public static string GetRoleForDisplay(this ClaimsPrincipal principal, IIndustry industry)
        {
            if (principal.IsInPartnerRole())
            {
                return "Partner";
            }

            if (principal.IsInBusinessCustomerManagerRole())
            {
                var organizationName = principal.FindFirstValue(Constants.ClaimTypes.OrganizationName);
                return $"{organizationName} Manager";
            }

            if (principal.IsInBusinessCustomerStockerRole())
            {
                var organizationName = principal.FindFirstValue(Constants.ClaimTypes.OrganizationName);
                return $"{organizationName} Stocker";
            }

            if (principal.IsInEmployeeRole())
            {
                return "Employee";
            }

            return industry?.IndividualCustomerAccountType;
        }

        public static bool IsInBusinessCustomerManagerRole(this ClaimsPrincipal principal)
        {
            return principal.IsInRole(Constants.Roles.BusinessCustomerManager);
        }

        public static bool IsInBusinessCustomerStockerRole(this ClaimsPrincipal principal)
        {
            return principal.IsInRole(Constants.Roles.BusinessCustomerStocker);
        }

        public static bool IsInEmployeeRole(this ClaimsPrincipal principal)
        {
            return principal.IsInRole(Constants.Roles.Employee);
        }

        public static bool IsInIndividualCustomerRole(this ClaimsPrincipal principal)
        {
            return principal.IsInRole(Constants.Roles.IndividualCustomer);
        }

        public static bool IsInPartnerRole(this ClaimsPrincipal principal)
        {
            return principal.IsInRole(Constants.Roles.Partner);
        }

        public static bool IsAllergicTo(this ClaimsPrincipal principal, string productAllergens)
        {
            if (!principal.Claims.Any(x => x.Type == "Allergens")
                || string.IsNullOrEmpty(productAllergens)
                || string.IsNullOrEmpty(principal.Claims.First(x => x.Type == "Allergens").Value)
            )
            {
                return false;
            }

            var productAllergenList = productAllergens.Split(",").ToList();

            var allergenClaim = principal.Claims.First(x => x.Type == "Allergens").Value.Split(",").ToList();

            return allergenClaim.Any(x => productAllergenList.Contains(x));
        }

        public static int GetUserProfilePercentage(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == "ProfileCompletion")
                && !string.IsNullOrEmpty(principal.Claims.First(x => x.Type == "ProfileCompletion").Value))
            {
                return Convert.ToInt32(principal.Claims.First(x => x.Type == "ProfileCompletion").Value);
            }

            return 0;
        }

        public static string GetPictureUrl(this ClaimsPrincipal principal)
        {
            if (!HasIdentityProvider(principal))
            {
                return string.Empty;
            }

            if (!HasIdpAccessToken(principal))
            {
                return string.Empty;
            }

            switch (GetIdentityProvider(principal))
            {
                case Constants.IdentityProvider.Facebook:

                    try
                    {
                        var client = new HttpClient();
                        var response = client.GetAsync("https://graph.facebook.com/me?fields=picture&access_token=" + GetAccessToken(principal))
                            .Result;
                        response.EnsureSuccessStatusCode();
                        var responseBody = response.Content.ReadAsStringAsync().Result;

                        var faceBookClaimResponse = JsonConvert.DeserializeObject<FaceBookClaimResponse>(responseBody);

                        if (faceBookClaimResponse == null || faceBookClaimResponse.picture == null || faceBookClaimResponse.picture.data == null ||
                            string.IsNullOrEmpty(faceBookClaimResponse.picture.data.url))
                        {
                            return string.Empty;
                        }

                        var imageByteArray = client.GetByteArrayAsync(faceBookClaimResponse.picture.data.url).Result;

                        if (imageByteArray == null)
                        {
                            return string.Empty;
                        }

                        return Convert.ToBase64String(imageByteArray);
                    }
                    catch
                    {
                        return string.Empty;
                    }
                default:
                    return string.Empty;
            }
        }

        public static string GetAccessToken(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == "idp_access_token"))
            {
                return principal.Claims.FirstOrDefault(x => x.Type == "idp_access_token").Value;
            }

            return string.Empty;
        }

        public static string GetIdentityProvider(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == Constants.ClaimTypes.IdentityProvider))
            {
                return principal.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.IdentityProvider).Value;
            }

            return string.Empty;
        }

        public static bool HasIdentityProvider(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == Constants.ClaimTypes.IdentityProvider))
            {
                return true;
            }

            return false;
        }

        public static string GetGroup(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == Constants.ClaimTypes.Group))
            {
                return principal.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Group).Value;
            }

            return string.Empty;
        }

        public static string GetObjectId(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == Constants.ClaimTypes.NameIdentifier))
            {
                return principal.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.NameIdentifier).Value;
            }

            return string.Empty;
        }

        public static bool HasGroup(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == Constants.ClaimTypes.Group))
            {
                return true;
            }

            return false;
        }

        public static bool IsSocialAccountLinked(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == Constants.ClaimTypes.LinkedSocialAccount))
            {
                return Convert.ToBoolean(principal.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.LinkedSocialAccount).Value);
            }

            return false;
        }

        public static bool HasIdpAccessToken(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == "idp_access_token"))
            {
                return true;
            }

            return false;
        }
    }

    public class IdentityService
    {
        private readonly IConfiguration _configuration;

        public IdentityService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IsUserLoggedIn(ClaimsPrincipal principal)
        {
            var config = AuthenticationBetaAppAccessOptions.Construct(_configuration);

            if (!config.RequireFullAppAuth)
            {
                return principal.Identity.IsAuthenticated;
            }

            if (!principal.Identity.IsAuthenticated)
            {
                return false;
            }

            var tenantId = principal.FindFirstValue(Constants.ClaimTypes.TenantIdentifier);

            return tenantId != config.TenantId;
        }
    }

    public class FaceBookClaimResponse
    {
        public Picture picture { get; set; }
    }

    public class Picture
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public string url { get; set; }
    }
}