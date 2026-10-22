
using AuthService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/database-check")]
    public class DatabaseCheckController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DatabaseCheckController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> CheckConnection()
        {
            try
            {
                bool connected =
                    await _context.Database.CanConnectAsync();

                if (connected)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "PostgreSQL connection successful."
                    });
                }

                return StatusCode(503, new
                {
                    success = false,
                    message = "Could not connect to PostgreSQL."
                });
            }
            catch (Exception)
            {
                // Avoid exposing connection strings or database credentials.
                return StatusCode(503, new
                {
                    success = false,
                    message = "Database connection failed. Check server configuration and logs."
                });
            }
        }
    }
}
