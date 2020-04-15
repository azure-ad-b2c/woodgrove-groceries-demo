namespace WoodGroveGroceriesWebApplication.Models.Settings
{
    public class TotpOptions : ConfigOptionsBase<TotpOptions>
    {
        public string Issuer { get; set; }

        public string AccountPrefix { get; set; }

        public int Timestep { get; set; }

        public string EncryptionKey { get; set; }

        protected override string SectionName => "Totp";
    }
}