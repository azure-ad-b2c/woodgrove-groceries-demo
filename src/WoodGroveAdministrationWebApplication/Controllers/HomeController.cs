using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WoodGroveAdministrationWebApplication.ViewModels;

namespace WoodGroveAdministrationWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly PowerBIReportOptions _options;

        public HomeController(IOptions<PowerBIReportOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _options = optionsAccessor.Value;
        }

        public IActionResult Index()
        {
            var viewModel = new HomeIndexViewModel
            {
                PowerBIReportPublishUrl = _options.PublishUrl
            };

            return View(viewModel);
        }

        public IActionResult Error()
        {
            var viewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            return View(viewModel);
        }
    }
}
