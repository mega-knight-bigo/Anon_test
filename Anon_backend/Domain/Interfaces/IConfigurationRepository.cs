using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Domain.Interfaces;

public interface IConfigurationRepository
{
    Task<IEnumerable<Configuration>> GetAllAsync(string? type = null);
    Task<Configuration?> GetByIdAsync(Guid id);
    Task<Configuration> CreateAsync(Configuration configuration);
    Task<Configuration> UpdateAsync(Configuration configuration);
    Task<bool> DeleteAsync(Guid id);
}
