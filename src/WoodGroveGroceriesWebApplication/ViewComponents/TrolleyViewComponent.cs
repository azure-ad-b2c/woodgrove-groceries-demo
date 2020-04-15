namespace WoodGroveGroceriesWebApplication.ViewComponents
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using ViewComponentModels;
    using ViewModels;
    using ViewServices;

    public class TrolleyViewComponent : ViewComponent
    {
        private readonly ITrolleyViewService _trolleyViewService;

        public TrolleyViewComponent(ITrolleyViewService trolleyViewService)
        {
            _trolleyViewService = trolleyViewService ?? throw new ArgumentNullException(nameof(trolleyViewService));
        }

        public async Task<IViewComponentResult> InvokeAsync(string ownerId)
        {
            var trolleyViewModel = await GetTrolleyViewModelAsync(ownerId);

            var trolleyViewComponentModel = new TrolleyViewComponentModel {ItemCount = trolleyViewModel.Items.Sum(item => item.Quantity)};

            return View(trolleyViewComponentModel);
        }

        private Task<TrolleyViewModel> GetTrolleyViewModelAsync(string ownerId)
        {
            return _trolleyViewService.GetOrCreateTrolleyForOwnerAsync(ownerId);
        }
    }
}