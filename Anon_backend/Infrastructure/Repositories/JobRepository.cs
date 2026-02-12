using Microsoft.EntityFrameworkCore;
using FullstackTemplate.Domain.Entities;
using FullstackTemplate.Domain.Interfaces;
using FullstackTemplate.Infrastructure.Data;

namespace FullstackTemplate.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly ApplicationDbContext _context;

    public JobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Job>> GetAllAsync()
    {
        return await _context.Jobs
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<Job?> GetByIdAsync(Guid id)
    {
        return await _context.Jobs.FindAsync(id);
    }

    public async Task<Job> CreateAsync(Job job)
    {
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<Job> UpdateAsync(Job job)
    {
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job is null) return false;
        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();
        return true;
    }
}
