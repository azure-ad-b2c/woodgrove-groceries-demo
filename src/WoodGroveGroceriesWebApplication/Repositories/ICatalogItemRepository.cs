using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.Entities;

namespace WoodGroveGroceriesWebApplication.Repositories
{
    public interface ICatalogItemRepository : IRepository<CatalogItem>
    {
        Task<CatalogItem> AddAsync(string ownerId, string productName, string productPictureUrl);
    }
}
