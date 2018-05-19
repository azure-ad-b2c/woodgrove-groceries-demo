using System;
using System.Linq.Expressions;
using WoodGroveGroceriesWebApplication.Entities;

namespace WoodGroveGroceriesWebApplication.Repositories.Specifications
{
    public class AllCatalogItemsSpecification : SpecificationBase<CatalogItem>
    {
        public AllCatalogItemsSpecification()
            : base(catalogItem => true)
        {
        }
    }
}
