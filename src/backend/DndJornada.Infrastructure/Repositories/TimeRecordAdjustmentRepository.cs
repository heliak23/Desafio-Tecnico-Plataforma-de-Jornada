using DndJornada.Domain.Entities;
using DndJornada.Domain.Enums;
using DndJornada.Domain.Repositories;
using DndJornada.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DndJornada.Infrastructure.Repositories;

public class TimeRecordAdjustmentRepository : ITimeRecordAdjustmentRepository
{
    private readonly AppDbContext _context;

    public TimeRecordAdjustmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TimeRecordAdjustment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TimeRecordAdjustments
            .Include(a => a.User)
            .Include(a => a.ReviewedBy)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<IReadOnlyList<TimeRecordAdjustment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.TimeRecordAdjustments
            .Include(a => a.ReviewedBy)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TimeRecordAdjustment>> GetPendingByManagerIdAsync(Guid managerId, CancellationToken ct = default)
    {
        return await _context.TimeRecordAdjustments
            .Include(a => a.User)
            .Where(a => a.Status == AdjustmentStatus.Pending && a.User!.ManagerId == managerId)
            .OrderBy(a => a.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TimeRecordAdjustment>> GetAllPendingAsync(CancellationToken ct = default)
    {
        return await _context.TimeRecordAdjustments
            .Include(a => a.User)
            .Where(a => a.Status == AdjustmentStatus.Pending)
            .OrderBy(a => a.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task AddAsync(TimeRecordAdjustment adjustment, CancellationToken ct = default)
    {
        await _context.TimeRecordAdjustments.AddAsync(adjustment, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TimeRecordAdjustment adjustment, CancellationToken ct = default)
    {
        _context.TimeRecordAdjustments.Update(adjustment);
        await _context.SaveChangesAsync(ct);
    }
}

