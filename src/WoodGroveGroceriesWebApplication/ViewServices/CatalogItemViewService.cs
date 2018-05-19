using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.Entities;
using WoodGroveGroceriesWebApplication.Extensions;
using WoodGroveGroceriesWebApplication.Repositories;
using WoodGroveGroceriesWebApplication.Repositories.Specifications;
using WoodGroveGroceriesWebApplication.ViewModels;

namespace WoodGroveGroceriesWebApplication.ViewServices
{
    public class CatalogItemViewService : ICatalogItemViewService
    {
        private readonly IRepository<CatalogItem> _catalogItemRepository;

        public CatalogItemViewService(IRepository<CatalogItem> catalogItemRepository)
        {
            _catalogItemRepository = catalogItemRepository ?? throw new ArgumentNullException(nameof(catalogItemRepository));
        }

        public async Task<IEnumerable<CatalogItemViewModel>> GetCatalogItemsAsync(ClaimsPrincipal user)
        {
            ISpecification<CatalogItem> listSpecification;

            if (user.IsInPartnerRole())
            {
                var ownerId = user.FindFirstValue(Constants.ClaimTypes.OwnerIdentifier);
                listSpecification = new CatalogItemForOwnerSpecification(ownerId);
            }
            else
            {
                listSpecification = new AllCatalogItemsSpecification();
            }

            var catalogItems = await _catalogItemRepository.ListAsync(listSpecification);

            return catalogItems.Select(catalogItem => new CatalogItemViewModel
            {
                Id = catalogItem.Id,
                OwnerId = catalogItem.OwnerId,
                ProductId = catalogItem.ProductId,
                ProductName = catalogItem.ProductName,
                ProductPictureUrl = catalogItem.ProductPictureUrl
            });
        }
    }
}
