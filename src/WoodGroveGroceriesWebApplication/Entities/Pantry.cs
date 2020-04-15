namespace WoodGroveGroceriesWebApplication.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Pantry : EntityBase
    {
        private readonly List<PantryItem> _items = new List<PantryItem>();

        public IReadOnlyCollection<PantryItem> Items => _items.AsReadOnly();

        public string OwnerId { get; set; }

        public void AddItem(string catalogItemId)
        {
            var existingItem = Items.FirstOrDefault(item => item.CatalogItemId == catalogItemId);

            if (existingItem != null)
            {
                return;
            }

            var newItem = new PantryItem {Id = Guid.NewGuid().ToString(), CatalogItemId = catalogItemId};

            _items.Add(newItem);
        }

        public void RemoveItem(string id)
        {
            var existingItem = Items.FirstOrDefault(item => item.Id == id);
            _items.Remove(existingItem);
        }
    }
}