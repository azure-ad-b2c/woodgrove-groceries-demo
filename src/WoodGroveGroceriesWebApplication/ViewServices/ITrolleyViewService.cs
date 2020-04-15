namespace WoodGroveGroceriesWebApplication.ViewServices
{
    using System.Threading.Tasks;
    using ViewModels;

    public interface ITrolleyViewService
    {
        Task<TrolleyViewModel> GetOrCreateTrolleyForOwnerAsync(string ownerId);
    }
}