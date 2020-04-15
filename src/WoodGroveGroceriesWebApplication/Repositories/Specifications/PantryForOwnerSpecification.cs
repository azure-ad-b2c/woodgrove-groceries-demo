namespace WoodGroveGroceriesWebApplication.Repositories.Specifications
{
    using Entities;

    public class PantryForOwnerSpecification : SpecificationBase<Pantry>
    {
        public PantryForOwnerSpecification(string ownerId)
            : base(pantry => pantry.OwnerId == ownerId)
        {
        }
    }
}