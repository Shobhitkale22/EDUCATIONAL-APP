using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth-test")]
    public class AuthTestController : ControllerBase
    {
        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            return Ok(new
            {
                message = "JWT authentication successful.",
                userId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                )?.Value,
                role = User.FindFirst(
                    System.Security.Claims.ClaimTypes.Role
                )?.Value
            });
        }
    }
}