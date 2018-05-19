using WoodGroveGroceriesWebApplication.Entities;

namespace WoodGroveGroceriesWebApplication.Repositories.Specifications
{
    public class PantryForOwnerSpecification : SpecificationBase<Pantry>
    {
        public PantryForOwnerSpecification(string ownerId)
            : base(pantry => pantry.OwnerId == ownerId)
        {
        }
    }
}
