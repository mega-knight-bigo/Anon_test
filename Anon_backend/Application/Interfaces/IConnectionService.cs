using FullstackTemplate.Application.DTOs;

namespace FullstackTemplate.Application.Interfaces;

public interface IConnectionService
{
    Task<IEnumerable<ConnectionDto>> GetAllAsync();
    Task<ConnectionDto?> GetByIdAsync(Guid id);
    Task<ConnectionDto> CreateAsync(CreateConnectionDto dto);
    Task<ConnectionDto?> UpdateAsync(Guid id, UpdateConnectionDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<TestConnectionResult> TestConnectionAsync(Guid id);
    Task<TestConnectionResult> TestConnectionConfigAsync(string type, System.Text.Json.JsonDocument config);
    Task<IEnumerable<ConnectionMetadataDto>> GetMetadataAsync(Guid id);
    Task<IEnumerable<ConnectionMetadataDto>> RefreshMetadataAsync(Guid id);
}
