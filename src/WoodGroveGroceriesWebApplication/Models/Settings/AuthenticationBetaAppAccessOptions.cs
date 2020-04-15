namespace WoodGroveGroceriesWebApplication.Models.Settings
{
    public class AuthenticationBetaAppAccessOptions : ConfigOptionsBase<AuthenticationBetaAppAccessOptions>
    {
        public bool RequireFullAppAuth { get; set; }

        public string Authority { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Policy { get; set; }

        public string TenantId { get; set; }

        protected override string SectionName => "Auth-BetaAccess";
    }
}