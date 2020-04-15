namespace WoodGroveGroceriesWebApplication.Models
{
    public class AppSettingsModel
    {
        public string SigningCertThumbprint { get; set; }
        public string TOTPIssuer { get; set; }
        public string TOTPAccountPrefix { get; set; }
        public int TOTPTimestep { get; set; }
        public string TOTPEncryptionKey { get; set; }
    }
}