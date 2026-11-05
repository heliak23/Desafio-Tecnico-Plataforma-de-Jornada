using DndJornada.Domain.Entities;
using DndJornada.Domain.Repositories;
using DndJornada.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DndJornada.Infrastructure.Repositories;

public class TimeRecordRepository : ITimeRecordRepository
{
    private readonly AppDbContext _context;

    public TimeRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TimeRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TimeRecords
            .Include(tr => tr.User)
            .FirstOrDefaultAsync(tr => tr.Id == id, ct);
    }

    public async Task<IReadOnlyList<TimeRecord>> GetByUserAndDateRangeUtcAsync(
        Guid userId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        return await _context.TimeRecords
            .Where(tr => tr.UserId == userId && tr.TimestampUtc >= startUtc && tr.TimestampUtc <= endUtc)
            .OrderBy(tr => tr.TimestampUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TimeRecord>> GetByUserAndDateAsync(
        Guid userId, DateOnly date, string timezoneId, CancellationToken ct = default)
    {
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        }
        catch
        {
            tz = TimeZoneInfo.Utc;
        }

        var startLocal = date.ToDateTime(TimeOnly.MinValue);
        var endLocal = date.ToDateTime(TimeOnly.MaxValue);

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);

        return await _context.TimeRecords
            .Where(tr => tr.UserId == userId && tr.TimestampUtc >= startUtc && tr.TimestampUtc <= endUtc)
            .OrderBy(tr => tr.TimestampUtc)
            .ToListAsync(ct);
    }

    public async Task<TimeRecord?> GetLastRecordByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.TimeRecords
            .Where(tr => tr.UserId == userId)
            .OrderByDescending(tr => tr.TimestampUtc)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(TimeRecord record, CancellationToken ct = default)
    {
        await _context.TimeRecords.AddAsync(record, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TimeRecord record, CancellationToken ct = default)
    {
        _context.TimeRecords.Update(record);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TimeRecord record, CancellationToken ct = default)
    {
        _context.TimeRecords.Remove(record);
        await _context.SaveChangesAsync(ct);
    }
}

