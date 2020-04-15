namespace WoodGroveGroceriesWebApplication.Controllers
{
    using System;
    using Managers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    /// <summary>
    /// The base controller is responsible for determining the industry based on the current set of cookies
    /// </summary>
    public class BaseController : Controller
    {
        public readonly IndustryManager _industryManager;

        public BaseController(IndustryManager industryManager)
        {
            _industryManager = industryManager;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _industryManager.SetIndustry(Request);

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var industry = _industryManager.GetIndustry();

            ViewBag.Industry = industry;

            base.OnActionExecuted(context);
        }

        public void CreateCookie(string key, string value)
        {
            var option = new CookieOptions();

            option.Expires = DateTime.Now.AddDays(7);

            Response.Cookies.Append(key, value ?? string.Empty, option);
        }

        public void RemoveCookie(string key)
        {
            var option = new CookieOptions();

            option.Expires = DateTime.Now.AddDays(-1);

            Response.Cookies.Append(key, string.Empty, option);
        }
    }
}