using static WoodGroveGroceriesWebApplication.Constants;

namespace WoodGroveGroceriesWebApplication.Controllers
{
    using System.Linq;
    using Extensions;
    using Managers;
    using Microsoft.AspNetCore.Mvc;
    using ViewModels;

    public class ConfigurationController : BaseController
    {
        private readonly PolicyManager _policyManager;

        public ConfigurationController(PolicyManager policyManager, IndustryManager industryManager) : base(industryManager)
        {
            _policyManager = policyManager;
        }

        public IActionResult Index()
        {
            return View(GetConfigurationViewModel());
        }

        public IActionResult Configure(ConfigurationViewModel configurationViewModel)
        {
            if (Request.Form.Any(x => x.Key == "setDefalut_action"))
            {
                RemoveCookie(DemoCookies.BgImageKey);
                RemoveCookie(DemoCookies.LogoImageKey);
                RemoveCookie(DemoCookies.DefaultSigninPolicyKey);
                RemoveCookie(DemoCookies.IndustryKey);
            }
            else if (Request.Form.Any(x => x.Key == "update_action"))
            {
                CreateCookie(DemoCookies.BgImageKey, configurationViewModel.BgImageUrl.ToBase64Encode());
                CreateCookie(DemoCookies.LogoImageKey, configurationViewModel.LogoImageUrl.ToBase64Encode());
                CreateCookie(DemoCookies.DefaultSigninPolicyKey, configurationViewModel.DefaultSUSIPolicy.ToBase64Encode());
                CreateCookie(DemoCookies.IndustryKey, configurationViewModel.Industry.ToBase64Encode());
            }

            ViewBag.Success = true;

            return RedirectToAction("Index");
        }

        private ConfigurationViewModel GetConfigurationViewModel()
        {
            var configurationViewModel = new ConfigurationViewModel();

            var bgImageUrlBase64 = Request.Cookies[DemoCookies.BgImageKey];
            var logoImageUrlBase64 = Request.Cookies[DemoCookies.LogoImageKey];
            var defaultSusiPolicy = Request.Cookies[DemoCookies.DefaultSigninPolicyKey];
            var industry = Request.Cookies[DemoCookies.IndustryKey];

            if (!string.IsNullOrEmpty(bgImageUrlBase64))
            {
                configurationViewModel.BgImageUrl = bgImageUrlBase64.ToBase64Decode();
            }

            if (!string.IsNullOrEmpty(logoImageUrlBase64))
            {
                configurationViewModel.LogoImageUrl = logoImageUrlBase64.ToBase64Decode();
            }

            if (!string.IsNullOrEmpty(defaultSusiPolicy))
            {
                configurationViewModel.DefaultSUSIPolicy = defaultSusiPolicy.ToBase64Decode();
            }

            configurationViewModel.PolicyList = _policyManager.PolicyList;
            configurationViewModel.IndustryList = _industryManager.IndustryList;
            configurationViewModel.Industry = industry?.ToBase64Decode() ?? _industryManager.IndustryList.First();

            return configurationViewModel;
        }
    }
}