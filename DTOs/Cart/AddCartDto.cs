namespace Ecommerce_web_api.DTOs.Cart
{
    public class AddCartDto
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public string Description { get; set; }
    }
}
