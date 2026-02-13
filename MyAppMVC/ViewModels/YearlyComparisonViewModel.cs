namespace MyAppMVC.ViewModels
{
    public class YearlyComparisonViewModel
    {
        public int Year { get; set; }
        public double Revenue { get; set; }
        public double Growth { get; set; }
        public int ItemsSold { get; set; }
        public int TransactionCount { get; set; }
        public double AvgRevenuePerItem { get; set; }
        public string Performance { get; set; } = "Average";
    }
}
