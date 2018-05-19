using System;
using System.Threading.Tasks;
using WoodGroveGroceriesWebApplication.Entities;
using WoodGroveGroceriesWebApplication.Repositories;

namespace WoodGroveGroceriesWebApplication.Managers
{
    public class TrolleyManager : ITrolleyManager
    {
        private readonly IRepository<Trolley> _trolleyRepository;

        public TrolleyManager(IRepository<Trolley> trolleyRepository)
        {
            _trolleyRepository = trolleyRepository ?? throw new ArgumentNullException(nameof(trolleyRepository));
        }

        public async Task AddToTrolleyAsync(string id, string catalogItemId, int quantity)
        {
            var trolley = await _trolleyRepository.GetAsync(id);
            trolley.AddItem(catalogItemId, quantity);
            await _trolleyRepository.UpdateAsync(trolley);
        }

        public async Task RemoveFromTrolleyAsync(string id, string itemId)
        {
            var trolley = await _trolleyRepository.GetAsync(id);
            trolley.RemoveItem(itemId);
            await _trolleyRepository.UpdateAsync(trolley);
        }
    }
}
