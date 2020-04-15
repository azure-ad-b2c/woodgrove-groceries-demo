namespace WoodGroveGroceriesWebApplication.Managers
{
    using System.Threading.Tasks;

    public interface ITrolleyManager
    {
        Task AddToTrolleyAsync(string id, string catalogItemId, int quantity);

        Task RemoveFromTrolleyAsync(string id, string itemId);

        Task RemoveAllItemFromTrolleyAsync(string id);
    }
}