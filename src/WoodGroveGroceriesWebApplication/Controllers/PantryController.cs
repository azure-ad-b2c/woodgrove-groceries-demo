namespace WoodGroveGroceriesWebApplication.Controllers
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Extensions;
    using Managers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using ViewModels;
    using ViewServices;

    [Authorize(Policy = Constants.AuthorizationPolicies.AccessPantry)]
    public class PantryController : BaseController
    {
        private readonly IPantryManager _pantryManager;
        private readonly IPantryViewService _pantryViewService;

        public PantryController(IPantryViewService pantryViewService, IPantryManager pantryManager, IndustryManager industryManager) :
            base(industryManager)
        {
            _pantryViewService = pantryViewService ?? throw new ArgumentNullException(nameof(pantryViewService));
            _pantryManager = pantryManager ?? throw new ArgumentNullException(nameof(pantryManager));
        }

        [Authorize(Policy = Constants.AuthorizationPolicies.AddToPantry)]
        [HttpPost]
        public async Task<IActionResult> AddToPantry(CatalogItemViewModel catalogItemViewModel)
        {
            var pantryViewModel = await GetPantryViewModelAsync();
            await _pantryManager.AddToPantryAsync(pantryViewModel.Id, catalogItemViewModel.Id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var userIsInBusinessAdministratorRole = User.IsInBusinessCustomerManagerRole();

            var viewModel = new PantryIndexViewModel
            {
                FormAspArea = "",
                FormAspController = userIsInBusinessAdministratorRole ? "Pantry" : "Trolley",
                FormAspAction = userIsInBusinessAdministratorRole ? "RemoveFromPantry" : "AddFromPantryToTrolley",
                FormSubmitButtonIconCssClass = userIsInBusinessAdministratorRole ? "fa fa-minus" : "fa fa-shopping-cart",
                FormSubmitButtonText = userIsInBusinessAdministratorRole ? "Remove from pantry" : "Add to cart",
                Pantry = await GetPantryViewModelAsync()
            };

            return View(viewModel);
        }

        [Authorize(Policy = Constants.AuthorizationPolicies.RemoveFromPantry)]
        [HttpPost]
        public async Task<IActionResult> RemoveFromPantry(string id)
        {
            var pantryViewModel = await GetPantryViewModelAsync();
            await _pantryManager.RemoveFromPantryAsync(pantryViewModel.Id, id);
            return RedirectToAction("Index");
        }

        private async Task<PantryViewModel> GetPantryViewModelAsync()
        {
            var organizationId = User.FindFirstValue(Constants.ClaimTypes.TenantIdentifier);
            return await _pantryViewService.GetOrCreatePantryForOwnerAsync(organizationId);
        }
    }
}