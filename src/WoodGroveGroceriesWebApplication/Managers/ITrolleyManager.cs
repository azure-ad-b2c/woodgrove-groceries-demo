using System.Threading.Tasks;

namespace WoodGroveGroceriesWebApplication.Managers
{
    public interface ITrolleyManager
    {
        Task AddToTrolleyAsync(string id, string catalogItemId, int quantity);

        Task RemoveFromTrolleyAsync(string id, string itemId);
    }
}
