using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.ViewModels;

namespace WoodGroveGroceriesWebApplication.ViewServices
{
    public interface IPantryViewService
    {
        Task<PantryViewModel> GetOrCreatePantryForOwnerAsync(string ownerId);
    }
}
