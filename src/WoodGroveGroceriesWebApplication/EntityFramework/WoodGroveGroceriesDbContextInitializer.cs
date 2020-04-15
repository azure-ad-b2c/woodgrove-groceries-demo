namespace WoodGroveGroceriesWebApplication.EntityFramework
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Entities;
    using Models.Settings;
    using Services;

    public static class WoodGroveGroceriesDbContextInitializer
    {
        public static async Task InitializeAsync(WoodGroveGroceriesDbContext dbContext, DbContextInitializationOptions dbContextInitializationOptions,
            HostService host)
        {
            if (!dbContext.CatalogItems.Any())
            {
                await InitializeCatalogItemsAsync(dbContext, dbContextInitializationOptions, host);
            }
        }

        private static Task InitializeCatalogItemsAsync(WoodGroveGroceriesDbContext dbContext,
            DbContextInitializationOptions dbContextInitializationOptions, HostService host)
        {
            var catalogItems = new List<CatalogItem>
            {
                new CatalogItem
                {
                    Id = "3696d034-deec-4ea3-8977-23558949b61c",
                    OwnerId = dbContextInitializationOptions.DefaultCatalogItemOwnerId,
                    ProductId = "abe5a167-8d20-4ad9-9f67-ad54b3e09ef2",
                    ProductName = "Apples",
                    ProductPictureUrl = $"{host.HostName}/images/apples.jpg"
                },
                new CatalogItem
                {
                    Id = "730b79be-9a4a-4221-a2e1-248d4ed785a9",
                    OwnerId = dbContextInitializationOptions.DefaultCatalogItemOwnerId,
                    ProductId = "0740b12d-c2db-44da-911e-8fd45a665d06",
                    ProductName = "Bananas",
                    ProductPictureUrl = $"{host.HostName}/images/bananas.jpg"
                },
                new CatalogItem
                {
                    Id = "11ed2c06-88a6-4bf2-ae63-6f8638ed6044",
                    OwnerId = dbContextInitializationOptions.DefaultCatalogItemOwnerId,
                    ProductId = "85ebc8d2-1912-488b-9925-2d5c2baaf26f",
                    ProductName = "Oranges",
                    ProductPictureUrl = $"{host.HostName}/images/oranges.jpg"
                },
                new CatalogItem
                {
                    Id = "a02af65f-507d-472d-be5f-e2f20fc94212",
                    OwnerId = dbContextInitializationOptions.DefaultCatalogItemOwnerId,
                    ProductId = "f5f808d9-0da5-4a6d-abf2-21582eec000f",
                    ProductName = "Milk",
                    ProductPictureUrl = $"{host.HostName}/images/milk-1056475.jpg",
                    ProductAllergyInfo = "Dairy"
                },
                new CatalogItem
                {
                    Id = "db02856c-21b7-4510-aa27-6862620c326b",
                    OwnerId = dbContextInitializationOptions.DefaultCatalogItemOwnerId,
                    ProductId = "c734e865-e376-4ef2-a1c1-1049da318278",
                    ProductName = "Bulk Nuts",
                    ProductPictureUrl = $"{host.HostName}/images/peanut-1328063.jpg",
                    ProductAllergyInfo = "Nuts"
                },
                new CatalogItem
                {
                    Id = "85dc4475-a56f-4e64-9877-1717a9622279",
                    OwnerId = dbContextInitializationOptions.DefaultCatalogItemOwnerId,
                    ProductId = "0d634083-de6b-46cc-ad2e-15cc96a15c01",
                    ProductName = "Bread",
                    ProductPictureUrl = $"{host.HostName}/images/spelt-bread-2-1326657.jpg",
                    ProductAllergyInfo = "Gluten"
                }
            };

            dbContext.AddRange(catalogItems);
            return dbContext.SaveChangesAsync();
        }
    }
}