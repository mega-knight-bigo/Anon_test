using FullstackTemplate.Application.DTOs;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Domain.Entities;
using FullstackTemplate.Domain.Interfaces;

namespace FullstackTemplate.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IActivityLogRepository _activityLogRepo;

    public UserService(IUserRepository userRepo, IActivityLogRepository activityLogRepo)
    {
        _userRepo = userRepo;
        _activityLogRepo = activityLogRepo;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(string? status = null)
    {
        var users = await _userRepo.GetAllAsync(status);
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepo.GetByEmailAsync(email);
        return user is null ? null : MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> SearchAsync(string query, string? status = null)
    {
        var users = await _userRepo.SearchAsync(query, status);
        return users.Select(MapToDto);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var user = new User
        {
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            ProfileImageUrl = dto.ProfileImageUrl,
            Name = dto.Name,
            Role = dto.Role ?? "viewer",
            Status = dto.Status ?? "active",
            InactiveDate = dto.InactiveDate,
            Department = dto.Department,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _userRepo.CreateAsync(user);

        await _activityLogRepo.CreateAsync(new ActivityLog
        {
            Action = "created",
            EntityType = "user",
            EntityId = created.Id.ToString(),
            EntityName = created.Name,
            Details = $"User {created.Name} created with role {created.Role}"
        });

        return MapToDto(created);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user is null) return null;

        if (dto.Email is not null) user.Email = dto.Email;
        if (dto.FirstName is not null) user.FirstName = dto.FirstName;
        if (dto.LastName is not null) user.LastName = dto.LastName;
        if (dto.ProfileImageUrl is not null) user.ProfileImageUrl = dto.ProfileImageUrl;
        if (dto.Name is not null) user.Name = dto.Name;
        if (dto.Role is not null) user.Role = dto.Role;
        if (dto.Status is not null) user.Status = dto.Status;
        if (dto.InactiveDate is not null) user.InactiveDate = dto.InactiveDate;
        if (dto.Department is not null) user.Department = dto.Department;

        var updated = await _userRepo.UpdateAsync(user);

        await _activityLogRepo.CreateAsync(new ActivityLog
        {
            Action = "updated",
            EntityType = "user",
            EntityId = updated.Id.ToString(),
            EntityName = updated.Name,
            Details = $"User {updated.Name} updated"
        });

        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user is null) return false;

        var deleted = await _userRepo.DeleteAsync(id);
        if (deleted)
        {
            await _activityLogRepo.CreateAsync(new ActivityLog
            {
                Action = "deleted",
                EntityType = "user",
                EntityId = id.ToString(),
                EntityName = user.Name,
                Details = $"User {user.Name} deleted"
            });
        }
        return deleted;
    }

    private static UserDto MapToDto(User u) => new(
        u.Id, u.Email, u.FirstName, u.LastName, u.ProfileImageUrl,
        u.Name, u.Role, u.Status, u.InactiveDate, u.Department,
        u.CreatedAt, u.UpdatedAt);
}
