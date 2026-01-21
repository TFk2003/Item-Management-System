namespace MyAppMVC.ViewModels
{
    public class MonthlyFinancialViewModel
    {
        public string Month { get; set; } = string.Empty;
        public double Revenue { get; set; }
        public int ItemsSold { get; set; }
        public int TransactionCount { get; set; }
    }
}
