namespace WoodGroveGroceriesWebApplication.Entities
{
    public class CatalogItem : EntityBase
    {
        public string OwnerId { get; set; }

        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductPictureUrl { get; set; }

        public string ProductAllergyInfo { get; set; }
    }
}