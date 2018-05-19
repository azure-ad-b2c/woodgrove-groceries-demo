using System;
using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.Entities;
using WoodGroveGroceriesWebApplication.EntityFramework;

namespace WoodGroveGroceriesWebApplication.Repositories
{
    public class CatalogItemDbRepository : DbRepository<CatalogItem>, ICatalogItemRepository
    {
        public CatalogItemDbRepository(WoodGroveGroceriesDbContext dbContext)
            : base(dbContext)
        {
        }

        public Task<CatalogItem> AddAsync(string ownerId, string productName, string productPictureUrl)
        {
            var catalogItem = new CatalogItem
            {
                OwnerId = ownerId,
                ProductId = Guid.NewGuid().ToString(),
                ProductName = productName,
                ProductPictureUrl = productPictureUrl
            };

            return AddAsync(catalogItem);
        }
    }
}
