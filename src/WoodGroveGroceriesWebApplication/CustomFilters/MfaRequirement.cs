namespace WoodGroveGroceriesWebApplication.CustomFilters
{
    using Microsoft.AspNetCore.Authorization;

    public class MfaRequirement : IAuthorizationRequirement
    {
        public MfaRequirement(string tfp)
        {
            Tfp = tfp;
        }

        public string Tfp { get; }
    }
}