using Microsoft.EntityFrameworkCore;
using FullstackTemplate.Domain.Entities;
using FullstackTemplate.Domain.Interfaces;
using FullstackTemplate.Infrastructure.Data;

namespace FullstackTemplate.Infrastructure.Repositories;

public class ConnectionRepository : IConnectionRepository
{
    private readonly ApplicationDbContext _context;

    public ConnectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Connection>> GetAllAsync()
    {
        return await _context.Connections
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Connection?> GetByIdAsync(Guid id)
    {
        return await _context.Connections.FindAsync(id);
    }

    public async Task<Connection> CreateAsync(Connection connection)
    {
        _context.Connections.Add(connection);
        await _context.SaveChangesAsync();
        return connection;
    }

    public async Task<Connection> UpdateAsync(Connection connection)
    {
        connection.UpdatedAt = DateTime.UtcNow;
        _context.Connections.Update(connection);
        await _context.SaveChangesAsync();
        return connection;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var connection = await _context.Connections.FindAsync(id);
        if (connection is null) return false;
        _context.Connections.Remove(connection);
        await _context.SaveChangesAsync();
        return true;
    }
}
