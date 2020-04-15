namespace WoodGroveGroceriesWebApplication.Models
{
    using System.Net;

    public class TOTPB2CResponse : B2CResponseContent
    {
        public TOTPB2CResponse(string message, HttpStatusCode status) : base(message, status)
        {
        }

        public string qrCodeBitmap { get; set; }
        public string secretKey { get; set; }
        public string timeStepMatched { get; set; }
    }
}