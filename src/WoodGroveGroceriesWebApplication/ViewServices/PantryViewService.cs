namespace WoodGroveGroceriesWebApplication.ViewServices
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Entities;
    using Repositories;
    using Repositories.Specifications;
    using ViewModels;

    public class PantryViewService : IPantryViewService
    {
        private readonly IRepository<CatalogItem> _catalogItemRepository;
        private readonly IRepository<Pantry> _pantryRepository;

        public PantryViewService(IRepository<Pantry> pantryRepository, IRepository<CatalogItem> catalogItemRepository)
        {
            _pantryRepository = pantryRepository ?? throw new ArgumentNullException(nameof(pantryRepository));
            _catalogItemRepository = catalogItemRepository ?? throw new ArgumentNullException(nameof(catalogItemRepository));
        }

        public async Task<PantryViewModel> GetOrCreatePantryForOwnerAsync(string ownerId)
        {
            var listSpecification = new PantryForOwnerSpecification(ownerId);

            var pantry = (await _pantryRepository.ListAsync(listSpecification)).FirstOrDefault();

            if (pantry == null)
            {
                pantry = await CreatePantryForOwnerAsync(ownerId);
            }

            return await CreatePantryViewModelFromPantryAsync(pantry);
        }

        private async Task<Pantry> CreatePantryForOwnerAsync(string ownerId)
        {
            var pantry = new Pantry {OwnerId = ownerId};

            await _pantryRepository.AddAsync(pantry);
            return pantry;
        }

        private async Task<PantryViewModel> CreatePantryViewModelFromPantryAsync(Pantry pantry)
        {
            var pantryViewModel = new PantryViewModel {Id = pantry.Id, OwnerId = pantry.OwnerId};

            foreach (var pantryItem in pantry.Items)
            {
                var pantryItemViewModel = new PantryItemViewModel {Id = pantryItem.Id, CatalogItemId = pantryItem.CatalogItemId};

                var catalogItem = await _catalogItemRepository.GetAsync(pantryItem.CatalogItemId);
                pantryItemViewModel.ProductId = catalogItem.ProductId;
                pantryItemViewModel.ProductName = catalogItem.ProductName;
                pantryItemViewModel.ProductPictureUrl = catalogItem.ProductPictureUrl;
                pantryViewModel.Items.Add(pantryItemViewModel);
            }

            return pantryViewModel;
        }
    }
}