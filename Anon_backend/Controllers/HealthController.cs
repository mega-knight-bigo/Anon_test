using Microsoft.AspNetCore.Mvc;
using FullstackTemplate.Infrastructure.Data;

namespace FullstackTemplate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            await _context.Database.CanConnectAsync();

            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Database = "Connected"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Database = "Disconnected",
                Error = ex.Message
            });
        }
    }
}
