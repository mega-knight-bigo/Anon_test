using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FullstackTemplate.Application.DTOs;
using FullstackTemplate.Application.Interfaces;

namespace FullstackTemplate.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanManageConnections")]
public class ConnectionsController : ControllerBase
{
    private readonly IConnectionService _service;

    public ConnectionsController(IConnectionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConnectionDto>>> GetAll()
    {
        var connections = await _service.GetAllAsync();
        return Ok(connections);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConnectionDto>> GetById(Guid id)
    {
        var connection = await _service.GetByIdAsync(id);
        if (connection is null) return NotFound(new { error = "Connection not found" });
        return Ok(connection);
    }

    [HttpPost]
    public async Task<ActionResult<ConnectionDto>> Create([FromBody] CreateConnectionDto dto)
    {
        var connection = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = connection.Id }, connection);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ConnectionDto>> Update(Guid id, [FromBody] UpdateConnectionDto dto)
    {
        var connection = await _service.UpdateAsync(id, dto);
        if (connection is null) return NotFound(new { error = "Connection not found" });
        return Ok(connection);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(new { error = "Connection not found" });
        return NoContent();
    }

    [HttpPost("{id}/test")]
    public async Task<ActionResult> Test(Guid id)
    {
        try
        {
            var result = await _service.TestConnectionAsync(id);
            return Ok(new { success = result.Success, message = result.Message, details = result.Details });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Connection not found" });
        }
    }

    [HttpPost("test-config")]
    public async Task<ActionResult> TestConfig([FromBody] TestConnectionConfigDto dto)
    {
        try
        {
            var result = await _service.TestConnectionConfigAsync(dto.Type, dto.Config);
            return Ok(new { success = result.Success, message = result.Message, details = result.Details });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Test failed", details = ex.Message });
        }
    }

    [HttpGet("{id}/metadata")]
    public async Task<ActionResult<IEnumerable<ConnectionMetadataDto>>> GetMetadata(Guid id)
    {
        var connection = await _service.GetByIdAsync(id);
        if (connection is null) return NotFound(new { error = "Connection not found" });
        var metadata = await _service.GetMetadataAsync(id);
        return Ok(metadata);
    }

    [HttpPost("{id}/refresh-metadata")]
    public async Task<ActionResult> RefreshMetadata(Guid id)
    {
        try
        {
            var metadata = await _service.RefreshMetadataAsync(id);
            return Ok(new { success = true, metadata, message = $"Fetched {metadata.Count()} objects" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Connection not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to refresh schema: {ex.Message}" });
        }
    }
}
