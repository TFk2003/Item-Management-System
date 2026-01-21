namespace MyAppMVC.ViewModels
{
    public class ClientSalesViewModel
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public double TotalPurchases { get; set; }
        public int PurchaseCount { get; set; }
    }
}
