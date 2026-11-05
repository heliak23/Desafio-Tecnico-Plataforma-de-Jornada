using DndJornada.Domain.Entities;
using DndJornada.Domain.Enums;

namespace DndJornada.Domain.Repositories;

public interface ITimeRecordAdjustmentRepository
{
    Task<TimeRecordAdjustment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TimeRecordAdjustment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<TimeRecordAdjustment>> GetPendingByManagerIdAsync(Guid managerId, CancellationToken ct = default);
    Task<IReadOnlyList<TimeRecordAdjustment>> GetAllPendingAsync(CancellationToken ct = default);
    Task AddAsync(TimeRecordAdjustment adjustment, CancellationToken ct = default);
    Task UpdateAsync(TimeRecordAdjustment adjustment, CancellationToken ct = default);
}

