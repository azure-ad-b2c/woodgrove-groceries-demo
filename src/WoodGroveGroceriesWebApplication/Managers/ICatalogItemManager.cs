using System.Threading.Tasks;

namespace WoodGroveGroceriesWebApplication.Managers
{
    public interface ICatalogItemManager
    {
        Task AddToCatalogAsync(string ownerId, string productName, string productPictureUrl);

        Task RemoveFromCatalogAsync(string id);
    }
}
