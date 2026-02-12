using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Domain.Interfaces;

public interface IConnectionRepository
{
    Task<IEnumerable<Connection>> GetAllAsync();
    Task<Connection?> GetByIdAsync(Guid id);
    Task<Connection> CreateAsync(Connection connection);
    Task<Connection> UpdateAsync(Connection connection);
    Task<bool> DeleteAsync(Guid id);
}
