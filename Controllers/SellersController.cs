using Ecommerce_web_api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellersController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public SellersController(ApplicationDbContext context)
        {
            _context = context;
            
        }


        [HttpGet("sellersData")]
        public async Task<IActionResult> GetAllSellers()
        {
            var sellers = await _context.Users
                .Where(u => u.Role == "Seller")
                .Include(u => u.Seller)
                .Select(u => new
                {
                    u.Id,
                    SellerName = u.Name,
                    ContactNumber = u.PhoneNumber,
                    u.Email,
                    ShopName = u.Seller.StoreName

                })
                .ToListAsync();

            return Ok(sellers);
        }

        [HttpPut("disable-seller/{id}")]
        public async Task<IActionResult> DisableSeller(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("User not found");

            user.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok("Seller disabled successfully");
        }

        [HttpPut("enable-seller/{id}")]
        public async Task<IActionResult> EnableSeller(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("seller not found");

            user.IsActive = true;

            await _context.SaveChangesAsync();

            return Ok("Seller enabled");
        }
    }


}
