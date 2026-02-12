using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FullstackTemplate.Application.DTOs;
using FullstackTemplate.Application.Interfaces;

namespace FullstackTemplate.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanManageUsers")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? search)
    {
        if (!string.IsNullOrEmpty(search))
        {
            var results = await _service.SearchAsync(search, status);
            return Ok(results);
        }
        var users = await _service.GetAllAsync(status);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user is null) return NotFound(new { error = "User not found" });
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        var user = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await _service.UpdateAsync(id, dto);
        if (user is null) return NotFound(new { error = "User not found" });
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(new { error = "User not found" });
        return Ok(new { success = true });
    }
}
