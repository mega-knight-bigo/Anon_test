using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FullstackTemplate.Application.DTOs;
using FullstackTemplate.Application.Interfaces;

namespace FullstackTemplate.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanManageJobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _service;

    public JobsController(IJobService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobDto>>> GetAll()
    {
        var jobs = await _service.GetAllAsync();
        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobDto>> GetById(Guid id)
    {
        var job = await _service.GetByIdAsync(id);
        if (job is null) return NotFound(new { error = "Job not found" });
        return Ok(job);
    }

    [HttpPost]
    public async Task<ActionResult<JobDto>> Create([FromBody] CreateJobDto dto)
    {
        var job = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<JobDto>> Update(Guid id, [FromBody] UpdateJobDto dto)
    {
        var job = await _service.UpdateAsync(id, dto);
        if (job is null) return NotFound(new { error = "Job not found" });
        return Ok(job);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(new { error = "Job not found" });
        return NoContent();
    }
}
