namespace WoodGroveGroceriesWebApplication.Managers
{
    using System.Threading.Tasks;

    public interface ICatalogItemManager
    {
        Task AddToCatalogAsync(string ownerId, string productName, string productPictureUrl);

        Task RemoveFromCatalogAsync(string id);
    }
}