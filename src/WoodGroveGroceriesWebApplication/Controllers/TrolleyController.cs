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
    [Authorize(Policy = Constants.AuthorizationPolicies.AccessTrolley)]
    public class TrolleyController : Controller
    {
        private readonly ITrolleyManager _trolleyManager;
        private readonly ITrolleyViewService _trolleyViewService;

        public TrolleyController(ITrolleyViewService trolleyViewService, ITrolleyManager trolleyManager)
        {
            _trolleyViewService = trolleyViewService ?? throw new ArgumentNullException(nameof(trolleyViewService));
            _trolleyManager = trolleyManager ?? throw new ArgumentNullException(nameof(trolleyManager));
        }

        [HttpPost]
        public async Task<IActionResult> AddFromCatalogToTrolley(CatalogItemViewModel catalogItemViewModel)
        {
            var trolleyViewModel = await GetTrolleyViewModelAsync();
            await _trolleyManager.AddToTrolleyAsync(trolleyViewModel.Id, catalogItemViewModel.Id, 1);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddFromPantryToTrolley(PantryItemViewModel pantryItemViewModel)
        {
            var trolleyViewModel = await GetTrolleyViewModelAsync();
            await _trolleyManager.AddToTrolleyAsync(trolleyViewModel.Id, pantryItemViewModel.CatalogItemId, 1);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var userIsInBusinessCustomerStockerRole = User.IsInBusinessCustomerStockerRole();

            var viewModel = new TrolleyIndexViewModel
            {
                AspController = userIsInBusinessCustomerStockerRole ? "Pantry" : "CatalogItem",
                Trolley = await GetTrolleyViewModelAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromTrolley(string id)
        {
            var trolleyViewModel = await GetTrolleyViewModelAsync();
            await _trolleyManager.RemoveFromTrolleyAsync(trolleyViewModel.Id, id);
            return RedirectToAction("Index");
        }

        private async Task<TrolleyViewModel> GetTrolleyViewModelAsync()
        {
            var userId = User.FindFirstValue(Constants.ClaimTypes.ObjectIdentifier);
            return await _trolleyViewService.GetOrCreateTrolleyForOwnerAsync(userId);
        }
    }
}
