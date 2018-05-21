using System.Threading.Tasks;

namespace WoodGroveGroceriesWebApplication.Managers
{
    public interface IPantryManager
    {
        Task AddToPantryAsync(string id, string catalogItemId);

        Task RemoveFromPantryAsync(string id, string itemId);
    }
}
