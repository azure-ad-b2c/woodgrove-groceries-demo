namespace WoodGroveGroceriesWebApplication.Controllers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using Services;

    /// <summary>
    /// This controller provides access for B2C to get the templates that it needs to display the various pages
    /// </summary>
    public class B2CUIController : Controller
    {
        private readonly HostService _host;

        public B2CUIController(HostService host)
        {
            _host = host;
        }

        [Route("Unified/{bkg?}/{logo?}")]
        public ActionResult Unified(string bkg, string logo)
        {
            if (bkg == null)
            {
                bkg = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        $"{_host.HostName}/images/groceries/background.jpg"));
            }

            ViewBag.Background = bkg;
            ViewBag.Logo = logo;

            return View();
        }

        public ActionResult GetB2CUI(string bkg, string logo, string type)
        {
            var locale = "en"; //replace when returning locale is implemented

            if (bkg == null)
            {
                bkg = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        $"{_host.HostName}/images/groceries/background.jpg"));
            }

            ViewBag.Background = bkg;
            ViewBag.Logo = logo;
            ViewBag.BaseUrl = _host.HostName;
            ViewBag.TabIcon = $"{_host.HostName}/images/woodgrove-favicon.ico";
            ViewBag.PageInstruction = GetPageInstruction(locale);

            switch ((type ?? string.Empty).ToLower())
            {
                case "editprofile":
                    return View("EditProfile");
                case "allergyinfo":
                    return View("AllergyInformation");
                case "mfa":
                    return View("MFA");
                case "toc":
                    return View("TOC");
                case "earegistration":
                    return View("EnterpriseAccountRegsitration");
                case "lalogin":
                    return View("LocalAccountLogin");
                case "laregistration":
                    return View("LocalAccountRegistration");
                case "saregistration":
                    return View("SocialAccountRegistration");
                case "saregistrationagegating":
                    return View("SocialAccountRegistrationAgeGating");
                case "laloginbyusername":
                    return View("LocalAccountLoginByUserName");
                case "inviteuserbyemail":
                    return View("InviteUsersByEmail");
                case "laregistrationbyusername":
                    return View("LocalAccountRegistrationByUserName");
                case "laregistrationbyusernameagegating":
                    return View("LocalAccountRegistrationByUserNameAgeGating");
                case "laprogressiveprofileagegating":
                    return View("LocalAccountProgressiveProfileAgeGating");
                case "laregistrationfrominvitation":
                    return View("LocalAccountRegistrationFromInvitation");
                case "laregistrationbyphonecode":
                    return View("LocalAccountRegistrationByPhoneCode");
                case "laloginbyphonecode":
                    return View("LocalAccountLoginByPhoneCode");
                case "forgotusername":
                    return View("ForgotUsername");
                case "forgotusernamemessage":
                    return View("ForgotUsernameMessage");
                case "laagegating":
                    return View("LocalAccountAgeGating");
                case "appfactorregistration":
                    return View("AppFactorRegistration");
                case "selfasserted":
                    return View("SelfAsserted");
                case "accountlinkingdraganddrop":
                    return View("AccountLinkingDragAndDrop");
                default:
                    return View("Unified");
            }
        }

        /// <summary>
        ///     This method provides a simple proxy for images from other locations.  This code should not be used in production.
        /// </summary>
        [Route("Image/{imageId}")]
        public ActionResult Image(string imageId)
        {
            var base64EncodedBytes = Convert.FromBase64String(imageId);
            var imageUrl = Encoding.UTF8.GetString(base64EncodedBytes);

            using (var client = new WebClient())
            {
                var data = client.DownloadData(imageUrl);

                Stream stream = new MemoryStream(data);
                return File(stream, "image/png");
            }
        }

        public static string GetPageInstruction(string locale)
        {
            switch (locale)
            {
                case "es":
                case "co":
                case "ts":
                    return "Arrastre y suelte para vincular o desvincular una cuenta social";
                case "fr":
                case "as":
                case "sw":
                    return "Faites glisser et déposez pour lier ou dissocier un compte social";
                default:
                    return "Drag and drop to link or un-link a social account";
            }
        }
    }
}