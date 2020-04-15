namespace WoodGroveGroceriesWebApplication.Models.Settings
{
    public class DbContextInitializationOptions : ConfigOptionsBase<DbContextInitializationOptions>
    {
        public string DefaultCatalogItemOwnerId { get; set; }

        protected override string SectionName => "DbContextInitialization";
    }
}