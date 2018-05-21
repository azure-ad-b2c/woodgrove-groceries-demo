using WoodGroveAdministrationWebApplication.ViewModels;

namespace WoodGroveAdministrationWebApplication.Extensions
{
    public static class UserExtensions
    {
        public static bool IsInBusinessCustomerManagerRole(this UserViewModel user)
        {
            return user.BusinessCustomerRole == Constants.BusinessCustomerRoles.Manager;
        }

        public static bool IsInBusinessCustomerStockerRole(this UserViewModel user)
        {
            return user.BusinessCustomerRole == Constants.BusinessCustomerRoles.Stocker;
        }
    }
}
