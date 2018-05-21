using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.Entities;

namespace WoodGroveGroceriesWebApplication.EntityFramework
{
    public static class WoodGroveGroceriesDbContextInitializer
    {
        public static async Task InitializeAsync(WoodGroveGroceriesDbContext dbContext, DbContextInitializationOptions dbContextInitializationOptions)
        {
            if (!dbContext.CatalogItems.Any())
            {
                await InitializeCatalogItemsAsync(dbContext, dbContextInitializationOptions);
            }
        }

        private static Task InitializeCatalogItemsAsync(WoodGroveGroceriesDbContext dbContext, DbContextInitializationOptions dbContextInitializationOptions)
        {
            var catalogItems = new List<CatalogItem>
            {
                new CatalogItem
                {
                    Id = "3696d034-deec-4ea3-8977-23558949b61c",
                    OwnerId = dbContextInitializationOptions.DefaultCatalogItemOwnerId,
                    ProductId = "abe5a167-8d20-4ad9-9f67-ad54b3e09ef2",
                    ProductName = "Apples",
                    ProductPictureUrl = "https://woodgrovegroceriesb2c.blob.core.windows.net/images/apples.jpg"
                },
                new CatalogItem
                {
                    Id = "730b79be-9a4a-4221-a2e1-248d4ed785a9",
                    OwnerId = dbContextInitializationOptions.DefaultCatalogItemOwnerId,
                    ProductId = "0740b12d-c2db-44da-911e-8fd45a665d06",
                    ProductName = "Bananas",
                    ProductPictureUrl = "https://woodgrovegroceriesb2c.blob.core.windows.net/images/bananas.jpg"
                },
                new CatalogItem
                {
                    Id = "11ed2c06-88a6-4bf2-ae63-6f8638ed6044",
                    OwnerId = dbContextInitializationOptions.DefaultCatalogItemOwnerId,
                    ProductId = "85ebc8d2-1912-488b-9925-2d5c2baaf26f",
                    ProductName = "Oranges",
                    ProductPictureUrl = "https://woodgrovegroceriesb2c.blob.core.windows.net/images/oranges.jpg"
                }
            };

            dbContext.AddRange(catalogItems);
            return dbContext.SaveChangesAsync();
        }
    }
}
