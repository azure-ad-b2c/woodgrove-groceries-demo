using System;
using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.Repositories;

namespace WoodGroveGroceriesWebApplication.Managers
{
    public class CatalogItemManager : ICatalogItemManager
    {
        private readonly ICatalogItemRepository _catalogItemRepository;

        public CatalogItemManager(ICatalogItemRepository catalogItemRepository)
        {
            _catalogItemRepository = catalogItemRepository ?? throw new ArgumentNullException(nameof(catalogItemRepository));
        }

        public async Task AddToCatalogAsync(string ownerId, string productName, string productPictureUrl)
        {
            await _catalogItemRepository.AddAsync(ownerId, productName, productPictureUrl);
        }

        public async Task RemoveFromCatalogAsync(string id)
        {
            await _catalogItemRepository.RemoveAsync(id);
        }
    }
}
