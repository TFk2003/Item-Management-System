using System.ComponentModel.DataAnnotations;

namespace MyAppMVC.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string CompanyName { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Phone]
        public required string PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public ICollection<Item>? Items { get; set; }

        public ICollection<PurchaseOrder>? PurchaseOrders { get; set; }
    }
}
