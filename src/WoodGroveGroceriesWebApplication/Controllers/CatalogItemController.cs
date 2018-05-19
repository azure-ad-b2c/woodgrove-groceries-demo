using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WoodGroveGroceriesWebApplication.Extensions;
using WoodGroveGroceriesWebApplication.Managers;
using WoodGroveGroceriesWebApplication.ViewModels;
using WoodGroveGroceriesWebApplication.ViewServices;

namespace WoodGroveGroceriesWebApplication.Controllers
{
    [Authorize(Policy = Constants.AuthorizationPolicies.AccessCatalog)]
    public class CatalogItemController : Controller
    {
        private readonly ICatalogItemManager _catalogItemManager;
        private readonly ICatalogItemViewService _catalogItemViewService;

        public CatalogItemController(ICatalogItemViewService catalogItemViewService, ICatalogItemManager catalogItemManager)
        {
            _catalogItemViewService = catalogItemViewService ?? throw new ArgumentNullException(nameof(catalogItemViewService));
            _catalogItemManager = catalogItemManager ?? throw new ArgumentNullException(nameof(catalogItemManager));
        }

        [Authorize(Policy = Constants.AuthorizationPolicies.AddToCatalog)]
        [HttpPost]
        public async Task<IActionResult> AddToCatalog(CatalogItemViewModel catalogItemViewModel)
        {
            var ownerId = User.FindFirstValue(Constants.ClaimTypes.OwnerIdentifier);
            await _catalogItemManager.AddToCatalogAsync(ownerId, catalogItemViewModel.ProductName, catalogItemViewModel.ProductPictureUrl);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var userIsInBusinessCustomerManagerRole = User.IsInBusinessCustomerManagerRole();
            var userIsInEmployeeRole = User.IsInEmployeeRole();
            var userIsInPartnerRole = User.IsInPartnerRole();

            var viewModel = new CatalogItemIndexViewModel
            {
                FormAspArea = string.Empty,
                FormAspController = userIsInPartnerRole || userIsInEmployeeRole ? "CatalogItem" : userIsInBusinessCustomerManagerRole ? "Pantry" : "Trolley",
                FormAspAction = userIsInPartnerRole || userIsInEmployeeRole ? "RemoveFromCatalog" : userIsInBusinessCustomerManagerRole ? "AddToPantry" : "AddFromCatalogToTrolley",
                FormSubmitButtonIconCssClass = userIsInPartnerRole || userIsInEmployeeRole ? "fa fa-minus" : userIsInBusinessCustomerManagerRole ? "fa fa-plus" : "fa fa-shopping-cart",
                FormSubmitButtonText = userIsInPartnerRole || userIsInEmployeeRole ? "Remove from catalog" : userIsInBusinessCustomerManagerRole ? "Add to pantry" : "Add to trolley",
                Items = await _catalogItemViewService.GetCatalogItemsAsync(User)
            };

            return View(viewModel);
        }

        [Authorize(Policy = Constants.AuthorizationPolicies.RemoveFromCatalog)]
        [HttpPost]
        public async Task<IActionResult> RemoveFromCatalog(string id)
        {
            await _catalogItemManager.RemoveFromCatalogAsync(id);
            return RedirectToAction("Index");
        }
    }
}
