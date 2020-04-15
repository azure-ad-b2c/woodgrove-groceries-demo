namespace WoodGroveGroceriesWebApplication.ViewServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Entities;
    using Extensions;
    using Managers;
    using Repositories;
    using Repositories.Specifications;
    using ViewModels;

    public class CatalogItemViewService : ICatalogItemViewService
    {
        private readonly IRepository<CatalogItem> _catalogItemRepository;
        private readonly IndustryManager _manager;

        public CatalogItemViewService(IRepository<CatalogItem> catalogItemRepository, IndustryManager manager)
        {
            _catalogItemRepository = catalogItemRepository ?? throw new ArgumentNullException(nameof(catalogItemRepository));
            _manager = manager;
        }

        public async Task<IEnumerable<CatalogItemViewModel>> GetCatalogItemsAsync(ClaimsPrincipal user)
        {
            ISpecification<CatalogItem> listSpecification;

            //if (user.IsInPartnerRole())
            //{
            //    var ownerId = user.FindFirstValue(Constants.ClaimTypes.OwnerIdentifier);
            //    listSpecification = new CatalogItemForOwnerSpecification(ownerId);
            //}
            //else
            //{
            //    listSpecification = new AllCatalogItemsSpecification();
            //}

            listSpecification = new AllCatalogItemsSpecification();

            var catalogItems = await _catalogItemRepository.ListAsync(listSpecification);

            return catalogItems.Select(catalogItem =>
            {
                var newItem = _manager.GetIndustry().ConvertItem(catalogItem);

                return new CatalogItemViewModel
                {
                    Id = newItem.Id,
                    OwnerId = newItem.OwnerId,
                    ProductId = newItem.ProductId,
                    ProductName = newItem.ProductName,
                    ProductPictureUrl = newItem.ProductPictureUrl,
                    ProductAllergyInfo = newItem.ProductAllergyInfo
                };
            });
        }
    }
}