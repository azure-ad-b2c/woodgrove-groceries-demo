using System;
using System.Linq.Expressions;
using WoodGroveGroceriesWebApplication.Entities;

namespace WoodGroveGroceriesWebApplication.Repositories.Specifications
{
    public class CatalogItemForOwnerSpecification : SpecificationBase<CatalogItem>
    {
        public CatalogItemForOwnerSpecification(string ownerId)
            : base(catalogItem => catalogItem.OwnerId == ownerId)
        {
        }
    }
}
