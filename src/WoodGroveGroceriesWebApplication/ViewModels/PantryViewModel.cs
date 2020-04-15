namespace WoodGroveGroceriesWebApplication.ViewModels
{
    using System.Collections.Generic;

    public class PantryViewModel
    {
        public string Id { get; set; }

        public IList<PantryItemViewModel> Items { get; set; } = new List<PantryItemViewModel>();

        public string OwnerId { get; set; }
    }
}