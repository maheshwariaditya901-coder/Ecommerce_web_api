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
    public class ProductsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        public ProductsController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct(Products product)
        {
            var seller = await _context.Users.FindAsync(product.UserId);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
       "amaheshwari819@gmail.com",
       "New Product Added",
       $"<h3>New product request send by seller<h3>" +
       $"<p><b>Name:</b> {product.Name}</p>" +
       $"<p><b>Price:</b> {product.Price}</p>" +
       $"<p><b>Seller:</b> {seller?.Name}</p>"

   );

            return Ok(product);
        }

        [HttpGet("Seller/{userId}")]
        public async Task<IActionResult> GetMyProducts(int userId)
        {
            var products = await _context.Products
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return Ok(products);
        }

        [HttpPut("EditProduct/{id}")]
        public async Task<IActionResult> EditProduct(int id , Products updatedProduct)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound("product not found");
            }

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;

            await _context.SaveChangesAsync(); 

            return Ok(product);

             
        }

        [HttpDelete("delete/{id}")]

        public async Task<IActionResult> deleteProduct(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound("no such product");
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return Ok("product deleted");
        }


        [HttpPatch("UpdateStatus/{id}")]
        public async Task<IActionResult> changeStatus(int id, string newstatus)
        {
            var product = await _context.Products
     .Include(p => p.User)
     .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound("no such product");

            product.Status = newstatus;
            await _context.SaveChangesAsync();

            try
            {
                string subject = $"Product {newstatus}";
                string body = $@"
            <h2>Product Status Updated</h2>
            <p>Hi {product.User?.Name},</p>
            <p>Your product <b>{product.Name}</b> has been <b>{newstatus}</b>.</p>
        ";

                await _emailService.SendEmailAsync(
                    product.User.Email,
                    subject,
                    body
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(new { message = "product status updated" });
        }


        [HttpGet("allProducts")]    
        public async Task<IActionResult> showProduct()
        {
            var products = await _context.Products.
                Include(p => p.User).
                Where(p => p.Status == "Approved" && p.User.IsActive == true).
                Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Stock,
                    sellerName = p.User.Name,
                    p.ImageUrl
                }).ToListAsync();


            return Ok(products);

        }

    }

}
