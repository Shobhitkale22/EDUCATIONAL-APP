
using AuthService.DTOs;
using Microsoft.AspNetCore.Mvc;


namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService.Services.AuthService _authService;

        public AuthController(
            AuthService.Services.AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                if (result.Message == "Invalid email or password.")
                    return Unauthorized(new { message = result.Message });

                if (result.Message == "Your account is blocked." ||
                    result.Message == "Your account is inactive." ||
                    result.Message.StartsWith("Only students"))
                {
                    return StatusCode(403, new
                    {
                        message = result.Message
                    });
                }

                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterStudentAsync(request);

            if (!result.Success)
            {
                if (result.Message ==
                    "An account with this email already exists.")
                {
                    return Conflict(new { message = result.Message });
                }

                if (result.Message == "Student role is not configured.")
                {
                    return StatusCode(500, new { message = result.Message });
                }

                return BadRequest(new { message = result.Message });
            }

            return StatusCode(201, new { message = result.Message });
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshAsync(request);

            if (!result.Success)
            {
                if (result.Message == "Invalid refresh token." ||
                    result.Message == "Invalid or expired refresh token.")
                {
                    return Unauthorized(new { message = result.Message });
                }

                if (result.Message ==
                    "Your account is blocked or inactive.")
                {
                    return StatusCode(403, new { message = result.Message });
                }

                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                data = result.Data
            });
        }



        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
            [FromBody] LogoutRequest request)
        {
            var result = await _authService.LogoutAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

    }
}
