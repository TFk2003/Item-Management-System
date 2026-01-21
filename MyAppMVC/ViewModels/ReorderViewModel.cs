using MyAppMVC.Models;

namespace MyAppMVC.ViewModels
{
    public class ReorderViewModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public int RecommendedReorder { get; set; }
        public int QuantityToOrder { get; set; }
        public int? SupplierId { get; set; }
        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();
    }
}
