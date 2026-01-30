using System.ComponentModel.DataAnnotations;
using MyAppMVC.Models;

namespace MyAppMVC.ViewModels
{
    public class PurchaseOrderViewModel
    {
        public int? Id { get; set; }
        public string? OrderNumber { get; set; }
        public int SupplierId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string Status { get; set; } = "Pending";
        public double TotalAmount { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; } = new List<PurchaseOrderItemDto>();
    }

    public class PurchaseOrderItemDto
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
    }
}
