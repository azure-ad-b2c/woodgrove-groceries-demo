namespace WoodGroveGroceriesWebApplication.Repositories.Specifications
{
    using Entities;

    public class TrolleyForOwnerSpecification : SpecificationBase<Trolley>
    {
        public TrolleyForOwnerSpecification(string ownerId)
            : base(trolley => trolley.OwnerId == ownerId)
        {
        }
    }
}