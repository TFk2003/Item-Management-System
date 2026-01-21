using MyAppMVC.Models;

namespace MyAppMVC.ViewModels
{
    public class ClientReportViewModel
    {
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public double TotalRevenue { get; set; }
        public double AveragePurchaseValue { get; set; }
        public List<Client> Clients { get; set; } = new();
        public List<ClientSummaryViewModel> TopClientsByValue { get; set; } = new();
    }
}
