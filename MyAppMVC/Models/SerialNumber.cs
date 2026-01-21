using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyAppMVC.Models
{
    public class SerialNumber
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public required string SerialCode { get; set; }
        [StringLength(100)]
        public string? Name { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? WarrantyInfo { get; set; }
        public int? ItemId { get; set; }
        [ForeignKey("ItemId")]
        public Item? Item { get; set; }
    }
}
