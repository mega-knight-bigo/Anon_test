using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FullstackTemplate.Application.DTOs;
using FullstackTemplate.Application.Interfaces;

namespace FullstackTemplate.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanReadActivity")]
public class ActivityController : ControllerBase
{
    private readonly IActivityLogService _service;

    public ActivityController(IActivityLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityLogDto>>> GetRecent([FromQuery] int limit = 50)
    {
        var logs = await _service.GetRecentAsync(limit);
        return Ok(logs);
    }
}
