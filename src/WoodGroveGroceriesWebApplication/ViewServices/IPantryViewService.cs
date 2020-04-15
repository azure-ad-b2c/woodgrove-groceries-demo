namespace WoodGroveGroceriesWebApplication.ViewServices
{
    using System.Threading.Tasks;
    using ViewModels;

    public interface IPantryViewService
    {
        Task<PantryViewModel> GetOrCreatePantryForOwnerAsync(string ownerId);
    }
}