using Ecommerce_web_api.Data;
using Ecommerce_web_api.Models;
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

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct(Products product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

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


    }
}
