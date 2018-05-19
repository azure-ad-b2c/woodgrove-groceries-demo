using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace WoodGroveAdministrationWebApplication.IdentityModel.Clients.ActiveDirectory
{
    public interface IAuthenticationContext
    {
        Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientCredential clientCredential);
    }
}
