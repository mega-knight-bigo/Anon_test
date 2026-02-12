using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Domain.Interfaces;
public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync(string? status = null);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> SearchAsync(string query, string? status = null);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid id);
}
