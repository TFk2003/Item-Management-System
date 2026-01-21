namespace MyAppMVC.ViewModels
{
    public class ClientSummaryViewModel
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public double TotalPurchases { get; set; }
        public int PurchaseCount { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
    }
}
