using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FullstackTemplate.Application.DTOs;
using FullstackTemplate.Application.Interfaces;

namespace FullstackTemplate.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanManageConfigurations")]
public class ConfigurationsController : ControllerBase
{
    private readonly IConfigurationService _service;

    public ConfigurationsController(IConfigurationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConfigurationDto>>> GetAll([FromQuery] string? type)
    {
        var configs = await _service.GetAllAsync(type);
        return Ok(configs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConfigurationDto>> GetById(Guid id)
    {
        var config = await _service.GetByIdAsync(id);
        if (config is null) return NotFound(new { error = "Configuration not found" });
        return Ok(config);
    }

    [HttpPost]
    public async Task<ActionResult<ConfigurationDto>> Create([FromBody] CreateConfigurationDto dto)
    {
        var config = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = config.Id }, config);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ConfigurationDto>> Update(Guid id, [FromBody] UpdateConfigurationDto dto)
    {
        var config = await _service.UpdateAsync(id, dto);
        if (config is null) return NotFound(new { error = "Configuration not found" });
        return Ok(config);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(new { error = "Configuration not found" });
        return NoContent();
    }
}
