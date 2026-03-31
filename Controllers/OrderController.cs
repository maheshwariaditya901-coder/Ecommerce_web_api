using Ecommerce_web_api.Data;
using Ecommerce_web_api.Models;
using Ecommerce_web_api.Services;
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
        private readonly EmailService _emailService;

        public OrderController(ApplicationDbContext context, EmailService emailService) 
            {
                _context = context;
                _emailService = emailService;

            }

            [HttpPost("checkout/{userId}")]
            public async Task<IActionResult> Checkout(int userId)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                var user = await _context.Users.FindAsync(userId);

                try
                {
                    //   Get cart items with product
                    var cartItems = await _context.CartItems
                        .Include(c => c.Product)
                        .Where(c => c.userId == userId)
                        .ToListAsync();

                    if (cartItems.Count == 0)
                        return BadRequest("Cart is empty");


                    var order = new Order
                    {
                        UserId = userId,
                        OrderDate = DateTime.Now,
                        TotalAmount = 0
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    decimal totalAmount = 0;


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

                    order.TotalAmount = totalAmount;


                    _context.CartItems.RemoveRange(cartItems);


                    await _context.SaveChangesAsync();


                    await transaction.CommitAsync();

                    try
                    {
                        
                    string body = $@"
        <h2>Order Confirmed 🎉</h2>
        <p>Hi {user.Name},</p>
        <p>Your order has been placed successfully.</p>
        <p><b>Order Id:</b> {order.Id}</p>
        <p><b>Total Amount:</b> ₹{order.TotalAmount}</p>";

                        await _emailService.SendEmailAsync(
                            user.Email,
                            "Order Confirmation",
                            body
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

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

