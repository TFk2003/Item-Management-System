namespace MyAppMVC.ViewModels
{
    public class ReportDashboardViewModel
    {
        public int TotalItems { get; set; }
        public int TotalClients { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int LowStockItems { get; set; }
        public int ActiveItems { get; set; }
        public int InactiveItems { get; set; }

        public double TotalInventoryValue { get; set; }
        public List<TransactionViewModel> RecentTransactions { get; set; } = new();
        public List<CategoryReportViewModel> TopCategories { get; set; } = new();
    }
}
