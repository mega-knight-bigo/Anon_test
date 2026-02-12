using FullstackTemplate.Application.DTOs;

namespace FullstackTemplate.Application.Interfaces;

public interface IConfigurationService
{
    Task<IEnumerable<ConfigurationDto>> GetAllAsync(string? type = null);
    Task<ConfigurationDto?> GetByIdAsync(Guid id);
    Task<ConfigurationDto> CreateAsync(CreateConfigurationDto dto);
    Task<ConfigurationDto?> UpdateAsync(Guid id, UpdateConfigurationDto dto);
    Task<bool> DeleteAsync(Guid id);
}
