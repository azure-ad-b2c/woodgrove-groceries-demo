namespace WoodGroveGroceriesWebApplication.Repositories
{
    using System;
    using System.Threading.Tasks;
    using Entities;
    using EntityFramework;

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
                OwnerId = ownerId, ProductId = Guid.NewGuid().ToString(), ProductName = productName, ProductPictureUrl = productPictureUrl
            };

            return AddAsync(catalogItem);
        }
    }
}