namespace WoodGroveGroceriesWebApplication.ViewServices
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Entities;
    using Managers;
    using Repositories;
    using Repositories.Specifications;
    using ViewModels;

    public class TrolleyViewService : ITrolleyViewService
    {
        private readonly IRepository<CatalogItem> _catalogItemRepository;
        private readonly IndustryManager _manager;
        private readonly IRepository<Trolley> _trolleyRepository;

        public TrolleyViewService(IRepository<Trolley> trolleyRepository, IRepository<CatalogItem> catalogItemRepository, IndustryManager manager)
        {
            _trolleyRepository = trolleyRepository ?? throw new ArgumentNullException(nameof(trolleyRepository));
            _catalogItemRepository = catalogItemRepository ?? throw new ArgumentNullException(nameof(catalogItemRepository));
            _manager = manager;
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
            var trolley = new Trolley {OwnerId = ownerId};

            await _trolleyRepository.AddAsync(trolley);
            return trolley;
        }

        private async Task<TrolleyViewModel> CreateTrolleyViewModelFromTrolleyAsync(Trolley trolley)
        {
            var trolleyViewModel = new TrolleyViewModel {Id = trolley.Id, OwnerId = trolley.OwnerId};

            foreach (var trolleyItem in trolley.Items)
            {
                var trolleyItemViewModel = new TrolleyItemViewModel
                {
                    Id = trolleyItem.Id, CatalogItemId = trolleyItem.CatalogItemId, Quantity = trolleyItem.Quantity
                };

                var catalogItem = await _catalogItemRepository.GetAsync(trolleyItem.CatalogItemId);

                var newItem = _manager.GetIndustry().ConvertItem(catalogItem);

                trolleyItemViewModel.ProductId = newItem.ProductId;
                trolleyItemViewModel.ProductName = newItem.ProductName;
                trolleyItemViewModel.ProductPictureUrl = newItem.ProductPictureUrl;
                trolleyItemViewModel.ProductAllergyInfo = newItem.ProductAllergyInfo;
                trolleyViewModel.Items.Add(trolleyItemViewModel);
            }

            return trolleyViewModel;
        }
    }
}