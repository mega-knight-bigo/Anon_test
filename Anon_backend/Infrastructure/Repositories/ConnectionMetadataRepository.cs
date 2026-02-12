using Microsoft.EntityFrameworkCore;
using FullstackTemplate.Domain.Entities;
using FullstackTemplate.Domain.Interfaces;
using FullstackTemplate.Infrastructure.Data;

namespace FullstackTemplate.Infrastructure.Repositories;

public class ConnectionMetadataRepository : IConnectionMetadataRepository
{
    private readonly ApplicationDbContext _context;

    public ConnectionMetadataRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConnectionMetadata>> GetByConnectionIdAsync(Guid connectionId)
    {
        return await _context.ConnectionMetadata
            .Where(m => m.ConnectionId == connectionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConnectionMetadata>> SaveAsync(Guid connectionId, IEnumerable<ConnectionMetadata> metadata)
    {
        // Delete existing metadata for this connection
        var existing = await _context.ConnectionMetadata
            .Where(m => m.ConnectionId == connectionId)
            .ToListAsync();
        _context.ConnectionMetadata.RemoveRange(existing);

        // Add new metadata
        var items = metadata.ToList();
        foreach (var item in items)
        {
            item.ConnectionId = connectionId;
        }
        _context.ConnectionMetadata.AddRange(items);
        await _context.SaveChangesAsync();
        return items;
    }

    public async Task<bool> DeleteByConnectionIdAsync(Guid connectionId)
    {
        var existing = await _context.ConnectionMetadata
            .Where(m => m.ConnectionId == connectionId)
            .ToListAsync();
        if (!existing.Any()) return false;
        _context.ConnectionMetadata.RemoveRange(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
