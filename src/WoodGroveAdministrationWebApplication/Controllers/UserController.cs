using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WoodGroveAdministrationWebApplication.Managers;
using WoodGroveAdministrationWebApplication.ViewModels;
using WoodGroveAdministrationWebApplication.ViewServices;

namespace WoodGroveAdministrationWebApplication.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserManager _userManager;
        private readonly IUserViewService _userViewService;

        public UserController(IUserViewService userViewService, IUserManager userManager)
        {
            _userViewService = userViewService ?? throw new ArgumentNullException(nameof(userViewService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpPost]
        public async Task<IActionResult> DemoteToStocker(string id)
        {
            await _userManager.DemoteToStockerAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new UserIndexViewModel
            {
                Users = await _userViewService.GetUsersAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PromoteToManager(string id)
        {
            await _userManager.PromoteToManagerAsync(id);
            return RedirectToAction("Index");
        }
    }
}
