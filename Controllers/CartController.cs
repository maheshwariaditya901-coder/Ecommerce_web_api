using Ecommerce_web_api.Data;
using Ecommerce_web_api.DTOs.Cart;
using Ecommerce_web_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase

    // inherit controllerbase because it give us method like ok , notfound , badrequest 
    // and only used for api.   
    {
        private readonly ApplicationDbContext _context;
        // its purpose is to access database

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // this is Dependency Injection (DI) which provide db to controller


        [HttpPost("AddProduct")]
        public async Task<IActionResult> addToCart(AddCartDto dto)
        {
            var existingItem = await _context.CartItems.FirstOrDefaultAsync(
                 c => c.userId == dto.UserId && c.ProductId == dto.ProductId
             );


            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    userId = dto.UserId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    Description = dto.Description,
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return Ok("item is added to cart");
        }

        [HttpGet("AddedCartProducts/{userId}")]

        public async Task<IActionResult> getAllCartProducts(int userId)
        {
            {
                var addedProducts = await _context.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.userId == userId)
                    .Select(c => new
                    {
                        c.Id,
                        c.Description,
                        ProductName = c.Product.Name,
                        c.Quantity,
                        c.Product.Price
                    }).ToListAsync();

                return Ok(addedProducts);
            }
        }

        [HttpDelete("RemoveItem/{userId}/{productId}")]
        public async Task<IActionResult> RemoveItem(int userId, int productId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.userId == userId && c.ProductId == productId);

            if (item == null)
            {
                return NotFound("Item not found");
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok("Item removed successfully");
        }

        [HttpDelete("clearCart/{userId}")]
        
        public async Task<IActionResult> clearCart(int userId)
        {
            var products = await _context.CartItems.
                Where(c => c.userId == userId).
                ToListAsync();

            if(products == null && products.Count == 0)
            {
                return NotFound("no product found");

            }

            _context.CartItems.RemoveRange(products);
            //  RemoveRange is used to remove multiple rows.
            await _context.SaveChangesAsync();

            return Ok("Cart cleared successfully");




        }
    }
}
