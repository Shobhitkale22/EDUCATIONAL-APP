using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // Expected values: Web or Mobile
        [Required]
        public string Platform { get; set; } = string.Empty;
        
    }
}