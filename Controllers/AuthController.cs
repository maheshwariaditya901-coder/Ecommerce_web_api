using Azure;
using Ecommerce_web_api.DTOs.Auth;
using Ecommerce_web_api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly EmailService _emailService;
        public AuthController(AuthService authService , EmailService emailService) { 
            _authService = authService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.Register(request);

            if (result != "Registration successful")
                return BadRequest(result);

            await _emailService.SendEmailAsync(
     request.Email,
     "Welcome to Our Platform 🎉",
     $"<h2>Welcome, {request.Name}!</h2>" +
     $"<p>Your account has been successfully created.</p>" +
     $"<p>You can now login and start using our platform.</p>"
 );


            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto request)
        {

            var result = await _authService.Login(request);

            if (string.IsNullOrEmpty(result.JwtToken))
            {
                return BadRequest(result.Message);
            }


            if (result.Message == "User not found" || result.Message == "Password is not correct")
                return BadRequest(result.Message);

            // Create secure Cookie Options
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,  
                Secure = false,  
                SameSite = SameSiteMode.Strict, 
                Expires = DateTime.UtcNow.AddHours(2)
            };

            // Attach the JWT to the response in a cookie named "jwtToken"
            Response.Cookies.Append("jwtToken", result.JwtToken, cookieOptions);

                
            return Ok(new
            {
                message = "Login successful",
                user = result.User
            });
        }




    }
}
