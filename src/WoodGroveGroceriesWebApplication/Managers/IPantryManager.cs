namespace WoodGroveGroceriesWebApplication.Managers
{
    using System.Threading.Tasks;

    public interface IPantryManager
    {
        Task AddToPantryAsync(string id, string catalogItemId);

        Task RemoveFromPantryAsync(string id, string itemId);
    }
}