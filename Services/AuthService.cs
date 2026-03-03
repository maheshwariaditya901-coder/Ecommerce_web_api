using Ecommerce_web_api.Data;
using Ecommerce_web_api.DTOs.Auth;
using Ecommerce_web_api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_web_api.Services
{
    public class AuthService
    {
        public readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> Register(RegisterUserDto request)
        { 
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

            if (existingUser != null)
                return "Email already registered";

            // Validate Role (VERY IMPORTANT)
            string[] allowedRoles = new string[] { "User", "Seller" };

            if (!allowedRoles.Contains(request.Role))
                return "Invalid role selected";


            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                PasswordHash = passwordHash
            };
 
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return "Registration successful";
        }
    }
}
