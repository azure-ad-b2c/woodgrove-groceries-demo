namespace WoodGroveGroceriesWebApplication.Repositories
{
    using System.Threading.Tasks;
    using Entities;

    public interface ICatalogItemRepository : IRepository<CatalogItem>
    {
        Task<CatalogItem> AddAsync(string ownerId, string productName, string productPictureUrl);
    }
}