using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.ViewModels;

namespace WoodGroveGroceriesWebApplication.ViewServices
{
    public interface ITrolleyViewService
    {
        Task<TrolleyViewModel> GetOrCreateTrolleyForOwnerAsync(string ownerId);
    }
}
