using System.ComponentModel.DataAnnotations.Schema;

namespace MyAppMVC.Models
{
    public class PurchaseOrderItem
    {
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }
        [ForeignKey("PurchaseOrderId")]
        public PurchaseOrder? PurchaseOrder { get; set; }

        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public Item? Item { get; set; }

        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice => Quantity * UnitPrice;
    }
}
