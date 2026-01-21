using MyAppMVC.Models;

namespace MyAppMVC.ViewModels
{
    public class InventoryReportViewModel
    {
        public List<Item> Items { get; set; } = new();
        public double TotalStockValue { get; set; }
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public List<Category> Categories { get; set; } = new();
        public List<CategoryStockViewModel> StockByCategory { get; set; } = new();
    }
}
