using MyAppMVC.Models;

namespace MyAppMVC.ViewModels
{
    public class SalesReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double TotalSales { get; set; }
        public int TotalItemsSold { get; set; }
        public double AverageSaleValue { get; set; }
        public List<ItemClient> SalesData { get; set; } = new();
        public List<ItemSalesViewModel> TopSellingItems { get; set; } = new();
        public List<ClientSalesViewModel> TopClients { get; set; } = new();
    }
}
