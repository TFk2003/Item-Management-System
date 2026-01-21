namespace MyAppMVC.ViewModels
{
    public class ItemSalesViewModel
    {
        public string ItemName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public double TotalRevenue { get; set; }
    }
}
