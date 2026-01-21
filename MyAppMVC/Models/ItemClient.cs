using System.ComponentModel.DataAnnotations.Schema;

namespace MyAppMVC.Models
{
    public class ItemClient
    {
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public Item? Item { get; set; }
        public int ClientId { get; set; }
        [ForeignKey("ClientId")]
        public Client? Client { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime PurchasedDate { get; set; } = DateTime.UtcNow;
        public double UnitPrice { get; set; }
        public double TotalPrice => Quantity * UnitPrice;
        public string? Notes { get; set; }
    }
}
