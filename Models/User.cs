using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ecommerce_web_api.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;  // Admin, Seller, User

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for seller
        public Seller Seller { get; set; }

        [JsonIgnore]
        public List<Products> Products { get; set; } 
    }
}