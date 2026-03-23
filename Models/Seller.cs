namespace Ecommerce_web_api.Models
{
    public class Seller
    {

        public int SellerId { get; set; } 
        public int UserId { get; set; } // FK to Users
        public string StoreName { get; set; }
         

        public string Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to User
        public User User { get; set; }
    }
}
