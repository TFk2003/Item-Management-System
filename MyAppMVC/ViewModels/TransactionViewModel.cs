namespace MyAppMVC.ViewModels
{
    public class TransactionViewModel
    {
        public string ItemName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double TotalPrice { get; set; }
        public DateTime Date { get; set; }
    }
}
