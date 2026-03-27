namespace Ecommerce_web_api.Models
{

    public class Order
    {
        public int Id { get; set; }          // Order ID

        public int UserId { get; set; }      // Who placed order

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }
 
        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
