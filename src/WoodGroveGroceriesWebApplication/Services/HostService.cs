namespace WoodGroveGroceriesWebApplication.Services
{
    using Microsoft.Extensions.Configuration;

    public class HostService
    {
        private readonly IConfiguration _config;

        public HostService(IConfiguration config)
        {
            _config = config;
        }

        public string HostName
        {
            get
            {
                var name = _config.GetValue<string>("WEBSITE_SITE_NAME");
                var slotname = _config.GetValue<string>("SlotName");
                if (string.IsNullOrEmpty(slotname))
                {
                    slotname = string.Empty;
                }

                var hostName = $"https://{name}{slotname}.azurewebsites.net";

                if (string.IsNullOrEmpty(name))
                {
                    var port = _config.GetValue<string>("HTTPS_PORT");
                    hostName = $"https://localhost:{port}";
                }

                return hostName;
            }
        }
    }
}