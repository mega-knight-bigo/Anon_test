using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Domain.Interfaces;

public interface IJobRepository
{
    Task<IEnumerable<Job>> GetAllAsync();
    Task<Job?> GetByIdAsync(Guid id);
    Task<Job> CreateAsync(Job job);
    Task<Job> UpdateAsync(Job job);
    Task<bool> DeleteAsync(Guid id);
}
