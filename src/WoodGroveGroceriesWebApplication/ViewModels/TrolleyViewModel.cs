namespace WoodGroveGroceriesWebApplication.ViewModels
{
    using System.Collections.Generic;

    public class TrolleyViewModel
    {
        public string Id { get; set; }

        public IList<TrolleyItemViewModel> Items { get; set; } = new List<TrolleyItemViewModel>();

        public string OwnerId { get; set; }
    }
}