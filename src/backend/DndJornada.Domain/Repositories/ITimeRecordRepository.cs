using DndJornada.Domain.Entities;

namespace DndJornada.Domain.Repositories;

public interface ITimeRecordRepository
{
    Task<TimeRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TimeRecord>> GetByUserAndDateRangeUtcAsync(Guid userId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<IReadOnlyList<TimeRecord>> GetByUserAndDateAsync(Guid userId, DateOnly date, string timezoneId, CancellationToken ct = default);
    Task<TimeRecord?> GetLastRecordByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(TimeRecord record, CancellationToken ct = default);
    Task UpdateAsync(TimeRecord record, CancellationToken ct = default);
    Task DeleteAsync(TimeRecord record, CancellationToken ct = default);
}

