using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace WoodGroveAdministrationWebApplication.IdentityModel.Clients.ActiveDirectory
{
    public class ActiveDirectoryAuthenticationContext : IAuthenticationContext
    {
        private readonly AuthenticationContext _authenticationContext;

        public ActiveDirectoryAuthenticationContext(IOptions<ActiveDirectoryGraphOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            var options = optionsAccessor.Value;
            _authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{options.TenantId}");
        }

        public Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientCredential clientCredential)
        {
            return _authenticationContext.AcquireTokenAsync(resource, clientCredential);
        }
    }
}
