namespace WoodGroveGroceriesWebApplication.Controllers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Models.Settings;
    using OtpSharp;
    using QRCoder;

    [Route("api/[controller]/[action]")]
    public class IdentityController : Controller
    {
        private readonly IConfiguration _config;
        private readonly TotpOptions _settings;

        public IdentityController(IConfiguration config)
        {
            _config = config;
            _settings = TotpOptions.Construct(_config);
        }

        [HttpPost(Name = "Generate")]
        public async Task<ActionResult> Generate([FromBody] TOTPInputModel inputClaims)
        {
            if (inputClaims == null)
            {
                return StatusCode((int) HttpStatusCode.Conflict, new TOTPB2CResponse("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            try
            {
                // Define the URL for the QR code. When user scan this URL, it opens one of the 
                // authentication apps running on the mobile device
                var secretKey = KeyGeneration.GenerateRandomKey(20);

                var TOTPUrl = KeyUrl.GetTotpUrl(secretKey, $"{_settings.AccountPrefix}:{inputClaims.userName}",
                    _settings.Timestep);

                TOTPUrl = $"{TOTPUrl}&issuer={_settings.Issuer.Replace(" ", "%20")}";

                // Generate QR code for the above URL
                var qrCodeGenerator = new QRCodeGenerator();
                var qrCodeData = qrCodeGenerator.CreateQrCode(TOTPUrl, QRCodeGenerator.ECCLevel.L);
                var qrCode = new BitmapByteQRCode(qrCodeData);
                var qrCodeBitmap = qrCode.GetGraphic(4);

                var output = new TOTPB2CResponse(string.Empty, HttpStatusCode.OK)
                {
                    qrCodeBitmap = Convert.ToBase64String(qrCodeBitmap), secretKey = EncryptAndBase64(Convert.ToBase64String(secretKey))
                };

                return Ok(output);
            }
            catch (Exception ex)
            {
                return StatusCode((int) HttpStatusCode.Conflict,
                    new TOTPB2CResponse($"General error (REST API): {ex.Message} - {ex.StackTrace}", HttpStatusCode.Conflict));
            }
        }

        [HttpPost(Name = "Verify")]
        public async Task<ActionResult> Verify([FromBody] TOTPInputModel inputClaims)
        {
            if (inputClaims == null)
            {
                return StatusCode((int) HttpStatusCode.Conflict, new TOTPB2CResponse("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            try
            {
                var secretKey = Convert.FromBase64String(DecryptAndBase64(inputClaims.secretKey));

                var totp = new Totp(secretKey);
                long timeStepMatched;

                // Verify the TOTP code provided by the users
                var verificationResult = totp.VerifyTotp(
                    inputClaims.totpCode,
                    out timeStepMatched,
                    VerificationWindow.RfcSpecifiedNetworkDelay);

                if (!verificationResult)
                {
                    return StatusCode((int) HttpStatusCode.Conflict,
                        new TOTPB2CResponse("The verification code is invalid.", HttpStatusCode.Conflict));
                }

                // Using the input claim 'timeStepMatched', we check whether the verification code has already been used.
                // For sign-up, the 'timeStepMatched' input claim is null and should not be evaluated 
                // For sign-in, the 'timeStepMatched' input claim contains the last time last matched (from the user profile), and evaluated with 
                // the value of the result of the TOTP out 'timeStepMatched' variable
                if (string.IsNullOrEmpty(inputClaims.timeStepMatched) == false &&
                    Convert.ToInt64(inputClaims.timeStepMatched ?? "0") >= timeStepMatched)
                {
                    return StatusCode((int) HttpStatusCode.Conflict,
                        new TOTPB2CResponse("The verification code has already been used.", HttpStatusCode.Conflict));
                }

                var output = new TOTPB2CResponse(string.Empty, HttpStatusCode.OK) {timeStepMatched = timeStepMatched.ToString()};

                return Ok(output);
            }
            catch (Exception ex)
            {
                return StatusCode((int) HttpStatusCode.Conflict,
                    new TOTPB2CResponse($"General error (REST API): {ex.Message}", HttpStatusCode.Conflict));
            }
        }

        public byte[] AesDelegator(byte[] inputBytes, Func<Aes, ICryptoTransform> transform)
        {
            byte[] output = null;

            var encryptionKey = _settings.EncryptionKey;
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey,
                    new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});
                if (encryptor != null)
                {
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);

                    using var ms = new MemoryStream();
                    using (var cs = new CryptoStream(ms, transform(encryptor), CryptoStreamMode.Write))
                    {
                        cs.Write(inputBytes, 0, inputBytes.Length);
                        cs.Close();
                    }

                    output = ms.ToArray();
                }
            }

            return output;
        }

        public string EncryptAndBase64(string encryptString)
        {
            var clearBytes = Encoding.Unicode.GetBytes(encryptString);

            var outputBytes = AesDelegator(clearBytes, aes => aes.CreateEncryptor());

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(outputBytes)));
        }

        public string DecryptAndBase64(string cipherText)
        {
            // Base64 decode
            cipherText = Encoding.UTF8.GetString(Convert.FromBase64String(cipherText));
            cipherText = cipherText.Replace(" ", "+");
            var cipherBytes = Convert.FromBase64String(cipherText);

            var outputBytes = AesDelegator(cipherBytes, aes => aes.CreateDecryptor());

            return Encoding.Unicode.GetString(outputBytes);
        }
    }
}