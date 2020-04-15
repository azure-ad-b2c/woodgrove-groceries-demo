namespace WoodGroveGroceriesWebApplication.ViewModels
{
    using System.Collections.Generic;

    public class ConfigurationViewModel
    {
        public string BgImageUrl { get; set; }

        public string LogoImageUrl { get; set; }

        public string DefaultSUSIPolicy { get; set; }

        public Dictionary<string, string> PolicyList { get; set; }

        public string Industry { get; set; }

        public List<string> IndustryList { get; set; }
    }
}