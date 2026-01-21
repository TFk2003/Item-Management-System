using System.ComponentModel.DataAnnotations;

namespace MyAppMVC.Models
{
    public class Client
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public required string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public required string LastName { get; set; }
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public required string Email { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
        [StringLength(200)]
        public string? Company { get; set; }
        [StringLength(500)]
        public string? Address { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public ICollection<ItemClient>? ItemClients { get; set; }
    }
}