using System.ComponentModel.DataAnnotations;

namespace MyAppMVC.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public ICollection<Item>? Items { get; set; }
    }
}
