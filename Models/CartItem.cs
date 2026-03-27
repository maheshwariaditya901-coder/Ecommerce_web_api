namespace Ecommerce_web_api.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int userId{ get; set; } // which user added this product 

        public int ProductId { get; set; }   

        // navigatio to product table
        public Products Product { get; set; }

        public int Quantity { get; set; }   

        public string Description { get; set; }
    }
}
