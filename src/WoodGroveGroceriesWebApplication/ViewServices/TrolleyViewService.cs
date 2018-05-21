using System;
using System.Linq;
using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.Entities;
using WoodGroveGroceriesWebApplication.Repositories;
using WoodGroveGroceriesWebApplication.Repositories.Specifications;
using WoodGroveGroceriesWebApplication.ViewModels;

namespace WoodGroveGroceriesWebApplication.ViewServices
{
    public class TrolleyViewService : ITrolleyViewService
    {
        private readonly IRepository<CatalogItem> _catalogItemRepository;
        private readonly IRepository<Trolley> _trolleyRepository;

        public TrolleyViewService(IRepository<Trolley> trolleyRepository, IRepository<CatalogItem> catalogItemRepository)
        {
            _trolleyRepository = trolleyRepository ?? throw new ArgumentNullException(nameof(trolleyRepository));
            _catalogItemRepository = catalogItemRepository ?? throw new ArgumentNullException(nameof(catalogItemRepository));
        }

        public async Task<TrolleyViewModel> GetOrCreateTrolleyForOwnerAsync(string ownerId)
        {
            var listSpecification = new TrolleyForOwnerSpecification(ownerId);

            var trolley = (await _trolleyRepository.ListAsync(listSpecification)).FirstOrDefault();

            if (trolley == null)
            {
                trolley = await CreateTrolleyForOwnerAsync(ownerId);
            }

            return await CreateTrolleyViewModelFromTrolleyAsync(trolley);
        }

        private async Task<Trolley> CreateTrolleyForOwnerAsync(string ownerId)
        {
            var trolley = new Trolley
            {
                OwnerId = ownerId
            };

            await _trolleyRepository.AddAsync(trolley);
            return trolley;
        }

        private async Task<TrolleyViewModel> CreateTrolleyViewModelFromTrolleyAsync(Trolley trolley)
        {
            var trolleyViewModel = new TrolleyViewModel
            {
                Id = trolley.Id,
                OwnerId = trolley.OwnerId
            };

            foreach (var trolleyItem in trolley.Items)
            {
                var trolleyItemViewModel = new TrolleyItemViewModel
                {
                    Id = trolleyItem.Id,
                    CatalogItemId = trolleyItem.CatalogItemId,
                    Quantity = trolleyItem.Quantity
                };

                var catalogItem = await _catalogItemRepository.GetAsync(trolleyItem.CatalogItemId);
                trolleyItemViewModel.ProductId = catalogItem.ProductId;
                trolleyItemViewModel.ProductName = catalogItem.ProductName;
                trolleyItemViewModel.ProductPictureUrl = catalogItem.ProductPictureUrl;
                trolleyViewModel.Items.Add(trolleyItemViewModel);
            }

            return trolleyViewModel;
        }
    }
}
