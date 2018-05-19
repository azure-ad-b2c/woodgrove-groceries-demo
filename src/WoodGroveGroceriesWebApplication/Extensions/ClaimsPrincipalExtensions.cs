using System.Security.Claims;

namespace WoodGroveGroceriesWebApplication.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetRoleForDisplay(this ClaimsPrincipal principal)
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

            return "Individual Customer";
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
    }
}
