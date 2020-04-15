namespace WoodGroveGroceriesWebApplication.Controllers
{
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Managers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using ViewModels;
    using ViewServices;

    [Authorize(Policy = Constants.AuthorizationPolicies.BetaAppAccess)]
    public class HomeController : BaseController
    {
        private readonly ICatalogItemViewService _catalogItemViewService;

        public HomeController(
            ICatalogItemViewService catalogItemViewService,
            IndustryManager manager) : base(manager)
        {
            _catalogItemViewService = catalogItemViewService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _catalogItemViewService.GetCatalogItemsAsync(User);

            var viewModel = new CatalogItemIndexViewModel {Items = items.Take(3)};

            return View(viewModel);
        }

        public IActionResult Error()
        {
            var viewModel = new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier};

            return View(viewModel);
        }

        [Authorize]
        public IActionResult Claims()
        {
            ViewData["Message"] = string.Format("Claims available for the user {0}", User.FindFirst("name")?.Value);
            return View();
        }
    }
}