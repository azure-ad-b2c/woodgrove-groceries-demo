namespace WoodGroveGroceriesWebApplication.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Trolley : EntityBase
    {
        private readonly List<TrolleyItem> _items = new List<TrolleyItem>();

        public IReadOnlyCollection<TrolleyItem> Items => _items.AsReadOnly();

        public string OwnerId { get; set; }

        public void AddItem(string catalogItemId, int quantity = 1)
        {
            var existingItem = Items.FirstOrDefault(item => item.CatalogItemId == catalogItemId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                return;
            }

            var newItem = new TrolleyItem {Id = Guid.NewGuid().ToString(), CatalogItemId = catalogItemId, Quantity = quantity};

            _items.Add(newItem);
        }

        public void RemoveItem(string id)
        {
            var existingItem = Items.FirstOrDefault(item => item.Id == id);
            _items.Remove(existingItem);
        }
    }
}