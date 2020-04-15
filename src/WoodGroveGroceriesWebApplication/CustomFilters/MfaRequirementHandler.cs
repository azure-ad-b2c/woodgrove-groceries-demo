namespace WoodGroveGroceriesWebApplication.CustomFilters
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class MfaRequirementHandler : AuthorizationHandler<MfaRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            MfaRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                var authFilterContext = context.Resource as AuthorizationFilterContext;
                authFilterContext.Result = new RedirectToActionResult("Login", "Account", null);

                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (!context.User.HasClaim(c => c.Type == Constants.ClaimTypes.TrustFrameworkPolicy && c.Value == requirement.Tfp))
            {
                var authFilterContext = context.Resource as AuthorizationFilterContext;
                authFilterContext.Result = new RedirectToActionResult("Mfa", "Account",
                    new {authenticationScheme = Constants.AuthenticationSchemes.CustomerAuth, policy = requirement.Tfp});

                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}