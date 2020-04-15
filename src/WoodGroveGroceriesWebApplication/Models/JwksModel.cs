namespace WoodGroveGroceriesWebApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;

    public class JwksModel
    {
        [JsonProperty("keys")]
        public ICollection<JwksKeyModel> Keys { get; set; }
    }

    public class JwksKeyModel
    {
        [JsonProperty("kid")]
        public string Kid { get; set; }

        [JsonProperty("nbf")]
        public long Nbf { get; set; }

        [JsonProperty("use")]
        public string Use { get; set; }

        [JsonProperty("kty")]
        public string Kty { get; set; }

        [JsonProperty("alg")]
        public string Alg { get; set; }

        [JsonProperty("x5c")]
        public ICollection<string> X5C { get; set; }

        [JsonProperty("x5t")]
        public string X5T { get; set; }

        [JsonProperty("n")]
        public string N { get; set; }

        [JsonProperty("e")]
        public string E { get; set; }

        public static JwksKeyModel FromSigningCredentials(X509SigningCredentials signingCredentials)
        {
            var certificate = signingCredentials.Certificate;

            // JWK cert data must be base64 (not base64url) encoded
            var certData = Convert.ToBase64String(certificate.Export(X509ContentType.Cert));

            // JWK thumbprints must be base64url encoded (no padding or special chars)
            var thumbprint = Base64UrlEncoder.Encode(certificate.GetCertHash());

            // JWK must have the modulus and exponent explicitly defined
            var rsa = certificate.PublicKey.Key as RSA;
            ;

            if (rsa == null)
            {
                throw new Exception("Certificate is not an RSA certificate.");
            }

            var keyParams = rsa.ExportParameters(false);
            var keyModulus = Base64UrlEncoder.Encode(keyParams.Modulus);
            var keyExponent = Base64UrlEncoder.Encode(keyParams.Exponent);

            return new JwksKeyModel
            {
                Kid = signingCredentials.Kid,
                Kty = "RSA",
                Nbf = new DateTimeOffset(certificate.NotBefore).ToUnixTimeSeconds(),
                Use = "sig",
                Alg = signingCredentials.Algorithm,
                X5C = new[] {certData},
                X5T = thumbprint,
                N = keyModulus,
                E = keyExponent
            };
        }
    }
}