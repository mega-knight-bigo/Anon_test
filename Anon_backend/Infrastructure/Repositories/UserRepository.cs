using Microsoft.EntityFrameworkCore;
using FullstackTemplate.Domain.Entities;
using FullstackTemplate.Domain.Interfaces;
using FullstackTemplate.Infrastructure.Data;

namespace FullstackTemplate.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync(string? status = null)
    {
        var query = _context.Users.AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(u => u.Status == status);
        return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> SearchAsync(string query, string? status = null)
    {
        var q = _context.Users.AsQueryable();
        q = q.Where(u =>
            (u.Name != null && EF.Functions.ILike(u.Name, $"%{query}%")) ||
            (u.Email != null && EF.Functions.ILike(u.Email, $"%{query}%")));
        if (!string.IsNullOrEmpty(status))
            q = q.Where(u => u.Status == status);
        return await q.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return false;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
