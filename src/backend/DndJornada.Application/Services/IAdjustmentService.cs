using DndJornada.Application.DTOs;

namespace DndJornada.Application.Services;

public interface IAdjustmentService
{
    Task<AdjustmentResponse> CreateRequestAsync(Guid userId, CreateAdjustmentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<AdjustmentResponse>> GetMyAdjustmentsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<AdjustmentResponse>> GetPendingForManagerAsync(Guid managerId, CancellationToken ct = default);
    Task<IReadOnlyList<AdjustmentResponse>> GetAllPendingAsync(CancellationToken ct = default);
    Task<AdjustmentResponse> ReviewAsync(Guid adjustmentId, Guid reviewerId, ReviewAdjustmentRequest request, CancellationToken ct = default);
}

