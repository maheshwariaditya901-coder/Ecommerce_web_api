using Ecommerce_web_api.Models;

namespace Ecommerce_web_api.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Message { get; set; }
        public string JwtToken { get; set; }
        public User User { get; set; }
    }
}
