using Ecommerce_web_api.Data;
using Ecommerce_web_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("checkout/{userId}")]
        public async Task<IActionResult> Checkout(int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //   Get cart items with product
                var cartItems = await _context.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.userId == userId)
                    .ToListAsync();

                if (cartItems.Count == 0)
                    return BadRequest("Cart is empty");

                //  Create Order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = 0
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();  

                decimal totalAmount = 0;

                //   Loop through cart items
                foreach (var item in cartItems)
                {
                    //  Check stock again 
                    if (item.Product.Stock < item.Quantity)
                        throw new Exception($"Not enough stock for {item.Product.Name}");

                    //  Reduce stock
                    item.Product.Stock -= item.Quantity;

                    //  Create OrderItem
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Price = item.Product.Price,             
                        Quantity = item.Quantity,
                        TotalPrice = item.Product.Price * item.Quantity
                    };

                    totalAmount += orderItem.TotalPrice;

                    _context.OrderItems.Add(orderItem);
                }

                //   Update total amount
                order.TotalAmount = totalAmount;

                //  Clear cart
                _context.CartItems.RemoveRange(cartItems);

                //  Save all changes
                await _context.SaveChangesAsync();

                //  Commit transaction
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Checkout successful",
                    orderId = order.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("orderHistory/{userId}")]
        public async Task<IActionResult> OrderHistory(int userId)
        {
            var orders = await _context.OrderItems
                .Include(o => o.Order)
                .Where(o => o.Order.UserId == userId)
                .Select(o => new
                {
                    o.OrderId,
                    o.ProductName,
                    o.Quantity,
                    o.Price,
                    o.TotalPrice,
                    OrderDate = o.Order.OrderDate
                })
                .ToListAsync();

            if (orders.Count == 0)
            {
                return NotFound("No orders placed yet");
            }

            return Ok(orders);
        }
    }
}
