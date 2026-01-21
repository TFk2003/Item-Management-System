using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyAppMVC.Models
{
    public class Item
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public required string Name { get; set; }
        [StringLength(1000)]
        public string? Description { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public double Price { get; set; }
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; } = 10;
        public string? SKU { get; set; }
        [StringLength(100)]
        public string? Manufacturer { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }
        public int? SerailId { get; set; }
        public SerialNumber? SerialNumber { get; set; }
        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
        public int? SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }
        public ICollection<ItemClient>? ItemClients { get; set; }
    }
}
