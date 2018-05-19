using WoodGroveGroceriesWebApplication.Entities;

namespace WoodGroveGroceriesWebApplication.Repositories.Specifications
{
    public class TrolleyForOwnerSpecification : SpecificationBase<Trolley>
    {
        public TrolleyForOwnerSpecification(string ownerId)
            : base(trolley => trolley.OwnerId == ownerId)
        {
        }
    }
}
