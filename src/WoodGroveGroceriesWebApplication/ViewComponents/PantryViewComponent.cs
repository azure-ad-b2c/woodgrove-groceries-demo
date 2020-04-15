namespace WoodGroveGroceriesWebApplication.ViewComponents
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using ViewComponentModels;
    using ViewModels;
    using ViewServices;

    public class PantryViewComponent : ViewComponent
    {
        private readonly IPantryViewService _pantryViewService;

        public PantryViewComponent(IPantryViewService pantryViewService)
        {
            _pantryViewService = pantryViewService ?? throw new ArgumentNullException(nameof(pantryViewService));
        }

        public async Task<IViewComponentResult> InvokeAsync(string ownerId)
        {
            var pantryViewModel = await GetPantryViewModelAsync(ownerId);

            var pantryViewComponentModel = new PantryViewComponentModel {ItemCount = pantryViewModel.Items.Count()};

            return View(pantryViewComponentModel);
        }

        private Task<PantryViewModel> GetPantryViewModelAsync(string ownerId)
        {
            return _pantryViewService.GetOrCreatePantryForOwnerAsync(ownerId);
        }
    }
}