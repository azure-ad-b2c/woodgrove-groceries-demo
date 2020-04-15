namespace WoodGroveGroceriesWebApplication.Repositories.Specifications
{
    using Entities;

    public class CatalogItemForOwnerSpecification : SpecificationBase<CatalogItem>
    {
        public CatalogItemForOwnerSpecification(string ownerId)
            : base(catalogItem => catalogItem.OwnerId == ownerId)
        {
        }
    }
}