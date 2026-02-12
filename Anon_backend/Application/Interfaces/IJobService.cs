using FullstackTemplate.Application.DTOs;

namespace FullstackTemplate.Application.Interfaces;

public interface IJobService
{
    Task<IEnumerable<JobDto>> GetAllAsync();
    Task<JobDto?> GetByIdAsync(Guid id);
    Task<JobDto> CreateAsync(CreateJobDto dto);
    Task<JobDto?> UpdateAsync(Guid id, UpdateJobDto dto);
    Task<bool> DeleteAsync(Guid id);
}
