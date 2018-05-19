namespace WoodGroveGroceriesWebApplication
{
    public static class Constants
    {
        public static class AuthenticationProperties
        {
            public const string Policy = "p";

            public const string UILocales = "ui_locales";
        }

        public static class AuthenticationSchemes
        {
            public const string B2BOpenIdConnect = "B2BOpenIdConnect";

            public const string B2COpenIdConnect = "B2COpenIdConnect";
        }

        public static class AuthorizationPolicies
        {
            public const string AccessCatalog = "AccessCatalog";

            public const string AddToCatalog = "AddToCatalog";

            public const string RemoveFromCatalog = "RemoveFromCatalog";

            public const string AccessPantry = "AccessPantry";

            public const string AddToPantry = "AddToPantry";

            public const string RemoveFromPantry = "RemoveFromPantry";

            public const string AccessTrolley = "AccessTrolley";
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
        }

        public static class Policies
        {
            public const string PasswordReset = "b2c_1_password_reset";

            public const string ProfileUpdateWithPersonalAccount = "b2c_1_profile_update_personal";

            public const string ProfileUpdateWithWorkAccount = "b2c_1a_profile_update_work";

            public const string SignUpOrSignInWithPersonalAccount = "b2c_1_sign_up_sign_in_personal";

            public const string SignUpOrSignInWithWorkAccount = "b2c_1a_sign_up_sign_in_work";
        }

        public static class Roles
        {
            public const string BusinessCustomerManager = "BusinessCustomerManager";

            public const string BusinessCustomerStocker = "BusinessCustomerStocker";

            public const string Employee = "Employee";

            public const string IndividualCustomer = "IndividualCustomer";

            public const string Partner = "Partner";
        }
    }
}
