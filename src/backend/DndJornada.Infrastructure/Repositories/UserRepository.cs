using DndJornada.Domain.Entities;
using DndJornada.Domain.Repositories;
using DndJornada.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DndJornada.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.Manager)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.Manager)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower(), ct);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.Manager)
            .OrderBy(u => u.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<User>> GetSubordinatesAsync(Guid managerId, CancellationToken ct = default)
    {
        return await _context.Users
            .Where(u => u.ManagerId == managerId && u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync(ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }
}

