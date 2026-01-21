namespace MyAppMVC.ViewModels
{
    public class FinancialReportViewModel
    {
        public int Year { get; set; }
        public double TotalRevenue { get; set; }
        public int TotalItemsSold { get; set; }
        public List<MonthlyFinancialViewModel> MonthlyData { get; set; } = new();
        public List<YearlyComparisonViewModel> YearlyComparison { get; set; } = new();
    }
}
