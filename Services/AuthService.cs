    using Ecommerce_web_api.Data;
    using Ecommerce_web_api.DTOs.Auth;
    using Ecommerce_web_api.Models;
    using Microsoft.EntityFrameworkCore;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;

namespace Ecommerce_web_api.Services
    {
    public class AuthService
    {
        public readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbContext context , IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<string> Register(RegisterUserDto request)
        {
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

            if (existingUser != null)
                return "Email already registered";

            // Validate Role (VERY IMPORTANT)
            string[] allowedRoles =  { "User", "Seller" , "Admin"};

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

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            if (request.Role == "Seller")
            {
                var seller = new Seller
                {
                     
                    StoreName = request.StoreName,
                    UserId = user.Id,
                    Address = request.Address
                };
                _context.Sellers.Add(seller);
                await _context.SaveChangesAsync();
            }

            return "Registration successful";
        }

        public async Task<LoginResponseDto> Login(LoginUserDto userExits)
        {
            var userExist = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userExits.Email);

            if (userExist == null)
                return new LoginResponseDto { Message = "User not found" };

            //  check for disabled account
            if (!userExist.IsActive)
            {
                return new LoginResponseDto
                {
                    Message = "Your account has been suspended. Please contact Admin."
                };
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                userExits.Password,
                userExist.PasswordHash
            );

            if (!isPasswordValid)
                return new LoginResponseDto { Message = "Password is not correct" };

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, userExist.Email),
        new Claim(ClaimTypes.Role, userExist.Role),
        new Claim(ClaimTypes.NameIdentifier, userExist.Id.ToString()) 
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new LoginResponseDto
            {
                Message = "Login successful",
                JwtToken = new JwtSecurityTokenHandler().WriteToken(token),
                User = userExist
            };
        }

    }
  }
