
using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class LogoutRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
