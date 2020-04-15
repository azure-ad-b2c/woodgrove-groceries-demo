namespace WoodGroveGroceriesWebApplication.ViewServices
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ViewModels;

    public interface ICatalogItemViewService
    {
        Task<IEnumerable<CatalogItemViewModel>> GetCatalogItemsAsync(ClaimsPrincipal user);
    }
}