using Microsoft.EntityFrameworkCore;
using FullstackTemplate.Domain.Entities;
using FullstackTemplate.Domain.Interfaces;
using FullstackTemplate.Infrastructure.Data;

namespace FullstackTemplate.Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly ApplicationDbContext _context;

    public ConfigurationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Configuration>> GetAllAsync(string? type = null)
    {
        var query = _context.Configurations.AsQueryable();
        if (!string.IsNullOrEmpty(type))
            query = query.Where(c => c.Type == type);
        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<Configuration?> GetByIdAsync(Guid id)
    {
        return await _context.Configurations.FindAsync(id);
    }

    public async Task<Configuration> CreateAsync(Configuration configuration)
    {
        _context.Configurations.Add(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }

    public async Task<Configuration> UpdateAsync(Configuration configuration)
    {
        configuration.UpdatedAt = DateTime.UtcNow;
        _context.Configurations.Update(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var config = await _context.Configurations.FindAsync(id);
        if (config is null) return false;
        _context.Configurations.Remove(config);
        await _context.SaveChangesAsync();
        return true;
    }
}
