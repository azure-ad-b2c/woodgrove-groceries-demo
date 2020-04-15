namespace WoodGroveGroceriesWebApplication.Repositories.Specifications
{
    using Entities;

    public class AllCatalogItemsSpecification : SpecificationBase<CatalogItem>
    {
        public AllCatalogItemsSpecification()
            : base(catalogItem => true)
        {
        }
    }
}