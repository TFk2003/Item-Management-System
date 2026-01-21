namespace MyAppMVC.ViewModels
{
    public class CategoryStockViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public double TotalValue { get; set; }
        public double AverageStock { get; set; }
    }
}
