using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.ViewModels;

namespace WoodGroveGroceriesWebApplication.ViewServices
{
    public interface ICatalogItemViewService
    {
        Task<IEnumerable<CatalogItemViewModel>> GetCatalogItemsAsync(ClaimsPrincipal user);
    }
}
