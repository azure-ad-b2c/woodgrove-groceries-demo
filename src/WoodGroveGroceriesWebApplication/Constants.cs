namespace WoodGroveGroceriesWebApplication
{
    public static class Constants
    {
        public static class AuthenticationType
        {
            public const string SignUp = "SignUp";

            public const string SignIn = "SignIn";
        }

        public static class AuthenticationProperties
        {
            public const string Policy = "p";

            public const string UILocales = "ui_locales";

            public const string BgImage = "bkg";

            public const string LogoImage = "logo";

            public const string InvitedEmail = "invitedEmail";

            public const string InvitedAccountId = "invitedAccountId";

            public const string InvitedGroupId = "invitedGroupId";

            public const string IdTokenHint = "id_token_hint";
        }

        public static class AuthenticationSchemes
        {
            public const string BetaAccessOpenIdConnect = "BetaAccessOpenIdConnect";

            public const string PartnerOpenIdConnect = "PartnerOpenIdConnect";

            public const string BusinessCustomerAuth = "BusinessCustomerAuth";

            public const string CustomerAuth = "CustomerAuth";
        }

        public static class AuthorizationPolicies
        {
            public const string BetaAppAccess = "BetaAppAccess";

            public const string AccessCatalog = "AccessCatalog";

            public const string AddToCatalog = "AddToCatalog";

            public const string RemoveFromCatalog = "RemoveFromCatalog";

            public const string AccessPantry = "AccessPantry";

            public const string AddToPantry = "AddToPantry";

            public const string RemoveFromPantry = "RemoveFromPantry";

            public const string AccessTrolley = "AccessTrolley";

            public const string AccessCheckout = "AccessCheckout";

            public const string ChangeUserRole = "ChangeUserRole";
        }

        public static class ClaimTypes
        {
            public const string BusinessCustomerRole = "business_customer_role";

            public const string IdentityProvider = "http://schemas.microsoft.com/identity/claims/identityprovider";

            public const string Issuer = "iss";

            public const string Name = "name";

            public const string ObjectIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";

            public const string OrganizationName = "organization_name";

            public const string OwnerIdentifier = "owner_id";

            public const string TenantIdentifier = "http://schemas.microsoft.com/identity/claims/tenantid";

            public const string TrustFrameworkPolicy = "tfp";

            public const string LinkedSocialAccount = "LinkedSocialAccount";

            public const string Group = "Group";

            public const string NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        }

        public static class Roles
        {
            public const string BusinessCustomerManager = "BusinessCustomerManager";

            public const string BusinessCustomerStocker = "BusinessCustomerStocker";

            public const string Employee = "Employee";

            public const string IndividualCustomer = "IndividualCustomer";

            public const string Partner = "Partner";
        }

        public static class DemoCookies
        {
            public const string BgImageKey = "BgImage";
            public const string LogoImageKey = "LogoImage";
            public const string DefaultSigninPolicyKey = "DefaultPolicy";
            public const string IndustryKey = "Industry";
            public const string UILocale = "UILocale";
        }

        public static class IdentityProvider
        {
            public const string Facebook = "facebook.com";
        }

        public class JsonResponse
        {
            public string state { get; set; }

            public string response { get; set; }
        }
    }
}