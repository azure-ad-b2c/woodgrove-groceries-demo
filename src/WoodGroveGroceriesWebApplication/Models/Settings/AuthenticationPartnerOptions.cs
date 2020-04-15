namespace WoodGroveGroceriesWebApplication.Models.Settings
{
    public class AuthenticationPartnerOptions : ConfigOptionsBase<AuthenticationPartnerOptions>
    {
        protected override string SectionName => "Auth-Partner";

        public PartnerExperience Experience { get; set; }

        public string Authority { get; set; }

        public string ClientId { get; set; }
    }

    public enum PartnerExperience
    {
        IGLM, // Identity Governance Lifecycle Management
        B2BSelfSignup
    }
}