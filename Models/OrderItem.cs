namespace Ecommerce_web_api.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }   // FK to Order table id

        public Order Order { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }  
            
        public int Quantity { get; set; }

        public decimal Price { get; set; }   // price per item

        public decimal TotalPrice { get; set; }
 
    }
}
