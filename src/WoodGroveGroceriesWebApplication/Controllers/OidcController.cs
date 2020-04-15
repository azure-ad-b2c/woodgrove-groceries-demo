namespace WoodGroveGroceriesWebApplication.Controllers
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using Models;
    using Newtonsoft.Json;

    /// <summary>
    /// The OIDC Controller provides an endpoint for B2C to refer to that enabled the use of a signed jwt id_token_hint parameter
    /// </summary>
    [Produces("application/json")]
    [Route("api/Oidc")]
    public class OidcController : Controller
    {
        private static Lazy<X509SigningCredentials> SigningCredentials;

        // Sample: Inject an instance of an AppSettingsModel class into the constructor of the consuming class, 
        // and let dependency injection handle the rest
        public OidcController(IConfiguration config)
        {
            var appSettingsModel = config.GetSection("AppSettings")
                .Get<AppSettingsModel>();

            // Sample: Load the certificate with a private key (must be pfx file)
            SigningCredentials = new Lazy<X509SigningCredentials>(() =>
            {
                var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadOnly);
                var certCollection = certStore.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    appSettingsModel.SigningCertThumbprint,
                    false);

                // Get the first cert with the thumb-print
                if (certCollection.Count > 0)
                {
                    return new X509SigningCredentials(certCollection[0]);
                }

                throw new Exception("Certificate not found");
            });
        }

        [Route(".well-known/openid-configuration", Name = "OIDCMetadata")]
        public ActionResult Metadata()
        {
            return Content(JsonConvert.SerializeObject(new OidcModel
            {
                // Sample: The issuer name is the application root path
                Issuer = $"{Request.Scheme}://{Request.Host}{Request.PathBase.Value}/",

                // Sample: Include the absolute URL to JWKs endpoint
                JwksUri = Url.Link("JWKS", null),

                // Sample: Include the supported signing algorithms
                IdTokenSigningAlgValuesSupported = new[] {SigningCredentials.Value.Algorithm}
            }), "application/json");
        }

        [Route(".well-known/keys", Name = "JWKS")]
        public ActionResult JwksDocument()
        {
            return Content(JsonConvert.SerializeObject(new JwksModel {Keys = new[] {JwksKeyModel.FromSigningCredentials(SigningCredentials.Value)}}),
                "application/json");
        }
    }
}