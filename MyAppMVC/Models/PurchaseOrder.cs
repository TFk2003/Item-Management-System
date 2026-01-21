using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyAppMVC.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string OrderNumber { get; set; }

        public int SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered, Cancelled

        public double TotalAmount { get; set; }

        public ICollection<PurchaseOrderItem>? PurchaseOrderItems { get; set; }
    }
}
